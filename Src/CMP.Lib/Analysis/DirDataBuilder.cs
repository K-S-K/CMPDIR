using System.Diagnostics;

using CMP.Lib.Data;
using CMP.Lib.Diagnostics;

namespace CMP.Lib.Analysis;

/// <summary>
/// The builder of the DirData from the file system
/// </summary>
public class DirDataBuilder
{
    private long DetectedFileCount = 0;
    private long ProcessedFileCount = 0;
    private long FailedFileCount = 0;

    private readonly List<string> Errors = [];
    private readonly bool ErrorHandlingTesting = false;

    private readonly IReportService _reportService;
    private readonly IProgressReporter _progressReporter;

    public DirDataBuilder(IReportService reportService, IProgressReporter progressReporter)
    {
        _reportService = reportService;
        _progressReporter = progressReporter;
    }


    public DirData BuildFromDirectory(string dirPath)
    {
        // Build the directory data structure
        DirData dirData = BuildFromDirectoryInternal(dirPath, TNL.Root);


        #region -> Calculate CRC32
        {
            Stopwatch swCalculate = Stopwatch.StartNew();

            using System.Timers.Timer timer = new(500);
            timer.Elapsed += (_, _) =>
            {
                _progressReporter.Report(new ProgressInfo(
                    "Processing files", ProcessedFileCount, DetectedFileCount));
            };
            timer.Start();

            CalculateCrc32(dirData, _reportService);

            swCalculate.Stop();
            _progressReporter.Clear();
            _reportService.Info($"Calculated checksums in {swCalculate.Elapsed.TotalSeconds:F3} s.");
        }
        #endregion


        #region -> Error Reporting
        if (Errors.Count > 0)
        {
            _reportService.Error($"{Environment.NewLine}Completed with {FailedFileCount} failed file(s).");
            int errorCounter = 0;
            foreach (var error in Errors)
            {
                _reportService.Error($"[{++errorCounter}]: {error}");
            }
            _reportService.Info("");
        }
        #endregion

        return dirData;
    }

    /// <summary>
    /// Build DirData from the specified directory path
    /// </summary>
    /// <param name="dirPath">The directory path to build from</param>
    /// <param name="nodeLevel">Whether this is the root directory</param>
    /// <param name="reportService">The report service for logging</param>
    /// <param name="progressReporter">The progress reporter for progress displaying</param>
    /// <param name="rootPath">The root path for relative path calculation</param>
    /// <returns>The built DirData</returns>
    private DirData BuildFromDirectoryInternal(string dirPath, TNL nodeLevel, string? rootPath = null)
    {
        if (nodeLevel == TNL.Root && rootPath == null)
        {
            rootPath = dirPath;
            _reportService.Info($"Scanning the directory: {rootPath}");
        }

        DirData dirData = new()
        {
            AbsoluteDirectoryPath = dirPath,
            RelativeDirectoryPath = nodeLevel == TNL.Root ? "/" : Path.GetRelativePath(rootPath!, dirPath)
        };

        Stopwatch swCollect = new();

        #region -> Collect Data
        {
            swCollect.Start();

            using System.Timers.Timer timer = new(500);
            if (nodeLevel == TNL.Root)
            {
                timer.Elapsed += (_, _) =>
                {
                    _progressReporter.Report(new ProgressInfo("Collecting files", DetectedFileCount));
                };
                timer.Start();
            }

            // Collect files
            List<FileData> files = [];
            string[] fileEntries = [];
            try
            {
                if (ErrorHandlingTesting && dirPath.Contains("Weather Station"))
                {
                    throw new Exception("Test exception at 'Weather Station'");
                }

                fileEntries = Directory.GetFiles(dirPath);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref FailedFileCount);
                Errors.Add($"Failed to get files from directory: {dirPath}. Reason: {ex.Message}");
            }

            foreach (string filePath in fileEntries)
            {
                if ( // Skip Windows Thumbs.db
                    filePath.EndsWith("Thumbs.db", StringComparison.OrdinalIgnoreCase) &&
                    Path.GetFileName(filePath).Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase)
                   )
                {
                    continue;
                }

                if ( // Skip MacOs .DS_Store
                    filePath.EndsWith(".DS_Store", StringComparison.OrdinalIgnoreCase) &&
                    Path.GetFileName(filePath).Equals(".DS_Store", StringComparison.OrdinalIgnoreCase)
                   )
                {
                    continue;
                }

                FileInfo fileInfo;

                try
                {
                    if (ErrorHandlingTesting && filePath.Contains("DSCF1898.jpg"))
                    {
                        throw new Exception("Test exception at 'DSCF1898.jpg'");
                    }

                    fileInfo = new(filePath);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref FailedFileCount);
                    Errors.Add($"Failed to get info for file: {filePath}. Reason: {ex.Message}");
                    continue;
                }

                FileData fileData = new()
                {
                    FileName = Path.GetFileName(filePath),
                    RelativeDirectoryPath = dirData.RelativeDirectoryPath,
                    Size = fileInfo.Length,
                };
                files.Add(fileData);

                // Calculate collecting files for progress
                Interlocked.Increment(ref DetectedFileCount);
            }
            dirData.Files = files.OrderBy(f => f.FileName).ToList();

            // Collect subdirectories
            List<DirData> subDirs = [];
            string[] subDirEntries = [];
            subDirEntries = Directory.GetDirectories(dirPath);

            try
            {
                if (ErrorHandlingTesting && dirPath.Contains("Entrance"))
                {
                    throw new Exception("Test exception at 'Entrance'");
                }

                subDirEntries = Directory.GetDirectories(dirPath);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref FailedFileCount);
                Errors.Add($"Failed to get subdirectories from directory: {dirPath}. Reason: {ex.Message}");
            }

            foreach (string subDirPath in subDirEntries)
            {
                DirData subDirData = BuildFromDirectoryInternal(subDirPath, TNL.Branch, rootPath);
                subDirs.Add(subDirData);
            }
            dirData.SubDirs = subDirs.OrderBy(d => d.DirName).ToList();

            timer.Stop();
            swCollect.Stop();
            if (nodeLevel == TNL.Root)
            {
                _progressReporter.Clear();
                _reportService.Info($"Collected {DetectedFileCount} files in {swCollect.ElapsedMilliseconds / 1000.0:F3} s.");
            }
        }
        #endregion


        return dirData;
    }

    private void CalculateCrc32(DirData data, IReportService reportService)
    {
        foreach (var file in data.Files)
        {
            string filePath = Path.Combine(data.AbsoluteDirectoryPath, file.FileName);

            try
            {
                if (ErrorHandlingTesting && ProcessedFileCount == 35)
                {
                    throw new Exception("Test exception at file 35");
                }
                file.CRC = Crc32.ComputeFile(filePath);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref FailedFileCount);
                Errors.Add($"Failed to calculate CRC32 for file: {filePath}. Reason: {ex.Message}");
            }

            // Calculate processed file count for progress
            Interlocked.Increment(ref ProcessedFileCount);
        }

        foreach (var subDir in data.SubDirs)
        {
            CalculateCrc32(subDir, reportService);
        }
    }
}
