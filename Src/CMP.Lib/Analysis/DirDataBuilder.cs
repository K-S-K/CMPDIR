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


    public DirData BuildFromDirectory(string dirPath, TNL nodeLevel, IReportService reportService, IProgressReporter progressReporter, string? rootPath = null)
    {
        DirData dirData = BuildFromDirectoryInternal(dirPath, nodeLevel, reportService, progressReporter, rootPath);

        Stopwatch swCalculate = new();

        #region -> Calculate CRC32
        {
            swCalculate.Start();

            using System.Timers.Timer timer = new(500);
            if (nodeLevel == TNL.Root)
            {
                timer.Elapsed += (_, _) =>
                {
                    progressReporter.Report(new ProgressInfo("Processing files", ProcessedFileCount, DetectedFileCount));
                };
                timer.Start();
            }

            CalculateCrc32(dirData);

            swCalculate.Stop();
            if (nodeLevel == TNL.Root)
            {
                progressReporter.Clear();
                reportService.Info($"Calculated CRC32 for directory: {dirPath} in {swCalculate.ElapsedMilliseconds / 1000.0:F3} s.");
            }
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
    private DirData BuildFromDirectoryInternal(string dirPath, TNL nodeLevel, IReportService reportService, IProgressReporter progressReporter, string? rootPath = null)
    {
        if (nodeLevel == TNL.Root && rootPath == null)
        {
            rootPath = dirPath;
            reportService.Info($"Set root path to: {rootPath}");
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
                    progressReporter.Report(new ProgressInfo("Scanning files", DetectedFileCount));
                };
                timer.Start();
            }

            // Collect files
            List<FileData> files = [];
            string[] fileEntries = Directory.GetFiles(dirPath);
            foreach (string filePath in fileEntries)
            {
                FileInfo fileInfo = new(filePath);
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
            string[] subDirEntries = Directory.GetDirectories(dirPath);
            foreach (string subDirPath in subDirEntries)
            {
                DirData subDirData = BuildFromDirectoryInternal(subDirPath, TNL.Branch, reportService, progressReporter, rootPath);
                subDirs.Add(subDirData);
            }
            dirData.SubDirs = subDirs.OrderBy(d => d.DirName).ToList();

            timer.Stop();
            swCollect.Stop();
            if (nodeLevel == TNL.Root)
            {
                progressReporter.Clear();
                reportService.Info($"Collected {DetectedFileCount} files in {swCollect.ElapsedMilliseconds / 1000.0:F3} s from the directory: {dirPath}");
            }
        }
        #endregion


        return dirData;
    }

    private void CalculateCrc32(DirData data)
    {
        foreach (var file in data.Files)
        {
            string filePath = Path.Combine(data.AbsoluteDirectoryPath, file.FileName);
            file.CRC = Crc32.ComputeFile(filePath);

            // Calculate processed file count for progress
            Interlocked.Increment(ref ProcessedFileCount);
        }

        foreach (var subDir in data.SubDirs)
        {
            CalculateCrc32(subDir);
        }
    }
}
