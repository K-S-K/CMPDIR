using System.Collections.Concurrent;
using System.Diagnostics;

using CMP.Lib.Data;
using CMP.Lib.Diagnostics;
using CMP.Lib.Analysis.FailureControl;

namespace CMP.Lib.Analysis;

/// <summary>
/// The builder of the DirData from the file system
/// </summary>
public class DirDataBuilder
{
    private long DetectedFileSize = 0;
    private long ProcessedFileSize = 0;
    private long DetectedFileCount = 0;
    private long ProcessedFileCount = 0;
    private long FailedFileCount = 0;
    private FileData? _currentFile = null;

    private readonly bool ErrorHandlingTesting = false;
    private readonly ConcurrentStack<AnalysisException> Errors = [];

    private readonly IReportService _reportService;
    private readonly IProgressReporter _progressReporter;

    public DirDataBuilder(IReportService reportService, IProgressReporter progressReporter)
    {
        _reportService = reportService;
        _progressReporter = progressReporter;
    }


    public DirData BuildFromDirectory(string dirPath, out IEnumerable<string> errors)
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
                    "Processed", swCalculate.Elapsed, ProcessedFileCount, DetectedFileCount, ProcessedFileSize, DetectedFileSize, _currentFile));
            };
            timer.Start();

            CalculateCrc32(dirData, _reportService);

            swCalculate.Stop();
            _progressReporter.Clear();
            _reportService.Info($"Calculated checksums during {ConsoleProgressReporter.DurationToStringMs(swCalculate.Elapsed)}");
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


        errors = Errors.Select(e => e.ToString()).ToList();
        return dirData;
    }

    private string[] GetDirectoryEntries(string dirPath)
    {
        string[] result;
        try
        {
            if (ErrorHandlingTesting && dirPath.Contains("Entrance"))
            {
                throw new Exception("Test exception at 'Entrance'");
            }

            result = Directory.GetDirectories(dirPath);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref FailedFileCount);
            throw new DirectoryListingException(ex, dirPath);
        }
        return result;
    }

    private string[] GetFileNames(string dirPath)
    {
        string[] result;
        try
        {
            if (ErrorHandlingTesting && dirPath.Contains("Weather Station"))
            {
                throw new Exception("Test exception at 'Weather Station'");
            }

            result = Directory.GetFiles(dirPath);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref FailedFileCount);
            throw new FileListingException(ex, dirPath);
        }
        return result;
    }

    private FileInfo GetFileInfo(string filePath)
    {
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
            throw new FileMetadataReadException(ex, filePath);
        }
        return fileInfo;
    }

    private uint ComputeCrc32(string filePath)
    {
        uint crc;
        try
        {
            if (ErrorHandlingTesting && ProcessedFileCount == 35)
            {
                throw new Exception("Test exception at file 35");
            }

            crc = Crc32.ComputeFile(filePath);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref FailedFileCount);
            throw new FileContentReadException(ex, filePath);
        }
        return crc;
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
                    _progressReporter.Report(new ProgressInfo("Collecting files", swCollect.Elapsed, DetectedFileCount, null, DetectedFileSize, null));
                };
                timer.Start();
            }

            // Collect files
            List<FileData> files = [];
            string[] fileNames = [];
            try
            {
                fileNames = GetFileNames(dirPath);
            }
            catch (AnalysisException ex)
            {
                Errors.Push(ex);
            }

            foreach (string filePath in fileNames)
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
                    fileInfo = GetFileInfo(filePath);
                }
                catch (AnalysisException ex)
                {
                    Errors.Push(ex);
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

                // Accumulate detected file size
                Interlocked.Add(ref DetectedFileSize, fileInfo.Length);
            }
            dirData.Files = files.OrderBy(f => f.FileName).ToList();

            // Collect subdirectories
            List<DirData> subDirs = [];
            string[] subDirEntries = [];

            try
            {
                subDirEntries = GetDirectoryEntries(dirPath);
            }
            catch (AnalysisException ex)
            {
                Interlocked.Increment(ref FailedFileCount);
                Errors.Push(ex);
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
                _reportService.Info($"Collected {DetectedFileCount} file(s) with total size {ConsoleProgressReporter.SizeWithSuffix(DetectedFileSize)} during {ConsoleProgressReporter.DurationToStringMs(swCollect.Elapsed)}");
            }
        }
        #endregion


        return dirData;
    }

    private void CalculateCrc32(DirData data, IReportService reportService)
    {
        foreach (FileData file in data.Files)
        {
            _currentFile = file;
            string filePath = Path.Combine(data.AbsoluteDirectoryPath, file.FileName);

            try
            {
                file.CRC = ComputeCrc32(filePath);
            }
            catch (AnalysisException ex)
            {
                Errors.Push(ex);
            }

            // Calculate processed file count for progress
            Interlocked.Increment(ref ProcessedFileCount);

            // Accumulate processed file size
            Interlocked.Add(ref ProcessedFileSize, file.Size);
        }

        foreach (var subDir in data.SubDirs)
        {
            CalculateCrc32(subDir, reportService);
        }
    }
}
