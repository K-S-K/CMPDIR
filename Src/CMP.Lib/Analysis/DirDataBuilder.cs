using System.Diagnostics;

using CMP.Lib.Data;
using CMP.Lib.Diagnostics;

namespace CMP.Lib.Analysis;

/// <summary>
/// The builder of the DirData from the file system
/// </summary>
public class DirDataBuilder
{
    private long DetectedFileSize = 0;
    private long ProcessedFileSize = 0;
    private long DetectedFileCount = 0;
    private FileData? _currentFile = null;

    private readonly FileSystem fileSystem = new();

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
                    "Processed", swCalculate.Elapsed, fileSystem.ProcessedFileCount, DetectedFileCount, ProcessedFileSize, DetectedFileSize, _currentFile));
            };
            timer.Start();

            CalculateCrc32(dirData, _reportService);

            swCalculate.Stop();
            _progressReporter.Clear();
            _reportService.Info($"Calculated checksums during {ProgressReporterConsole.DurationToStringMs(swCalculate.Elapsed)}");
        }
        #endregion


        #region -> Error Reporting
        if (fileSystem.Errors.Count() > 0)
        {
            _reportService.Error($"{Environment.NewLine}Completed with {fileSystem.FailedFileCount} failed file(s).");
            int errorCounter = 0;
            foreach (var error in fileSystem.Errors)
            {
                _reportService.Error($"[{++errorCounter}]: {error}");
            }
            _reportService.Info("");
        }
        #endregion


        errors = fileSystem.Errors.Select(e => e.ToString()).ToList();
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
                    _progressReporter.Report(new ProgressInfo("Collecting files", swCollect.Elapsed, DetectedFileCount, null, DetectedFileSize, null));
                };
                timer.Start();
            }

            // Collect files
            List<FileData> files = [];
            fileSystem.TryGetFileNames(dirPath, out string[] fileNames);

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

                if (!fileSystem.TryGetFileInfo(filePath, out fileInfo!))
                {
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

            fileSystem.TryGetDirectoryEntries(dirPath, out subDirEntries);

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
                _reportService.Info($"Collected {DetectedFileCount} file(s) with total size {ProgressReporterConsole.SizeWithSuffix(DetectedFileSize)} during {ProgressReporterConsole.DurationToStringMs(swCollect.Elapsed)}");
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

            fileSystem.TryComputeCrc32(filePath, out uint crc);
            file.CRC = crc;

            // Accumulate processed file size
            Interlocked.Add(ref ProcessedFileSize, file.Size);
        }

        foreach (var subDir in data.SubDirs)
        {
            CalculateCrc32(subDir, reportService);
        }
    }
}
