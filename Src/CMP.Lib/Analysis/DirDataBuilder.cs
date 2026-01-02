using System.Diagnostics;

using CMP.Lib.Data;
using CMP.Lib.Diagnostics;

namespace CMP.Lib.Analysis;

/// <summary>
/// The builder of the DirData from the file system
/// </summary>
public static class DirDataBuilder
{
    /// <summary>
    /// Build DirData from the specified directory path
    /// </summary>
    /// <param name="dirPath">The directory path to build from</param>
    /// <param name="nodeLevel">Whether this is the root directory</param>
    /// <param name="reportService">The report service for logging</param>
    /// <param name="progressReporter">The progress reporter</param>
    /// <param name="rootPath">The root path for relative path calculation</param>
    /// <returns>The built DirData</returns>
    public static DirData BuildFromDirectory(string dirPath, TNL nodeLevel, IReportService reportService, IProgressReporter progressReporter, string? rootPath = null)
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
        Stopwatch swCalculate = new();

        #region -> Collect Data
        {
            swCollect.Start();

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
            }
            dirData.Files = files.OrderBy(f => f.FileName).ToList();

            // Collect subdirectories
            List<DirData> subDirs = [];
            string[] subDirEntries = Directory.GetDirectories(dirPath);
            foreach (string subDirPath in subDirEntries)
            {
                DirData subDirData = BuildFromDirectory(subDirPath, TNL.Branch, reportService, progressReporter, rootPath);
                subDirs.Add(subDirData);
            }
            dirData.SubDirs = subDirs.OrderBy(d => d.DirName).ToList();

            swCollect.Stop();
            if (nodeLevel == TNL.Root)
            {
                reportService.Info($"Collected data from directory: {dirPath} in {swCollect.ElapsedMilliseconds / 1000.0:F3} s.");
            }
        }
        #endregion


        #region -> Calculate CRC32
        {
            swCalculate.Start();

            CalculateCrc32(dirData);

            swCalculate.Stop();
            if (nodeLevel == TNL.Root)
            {
                reportService.Info($"Calculated CRC32 for directory: {dirPath} in {swCalculate.ElapsedMilliseconds / 1000.0:F3} s.");
            }
        }
        #endregion


        return dirData;
    }

    private static void CalculateCrc32(DirData data)
    {
        foreach (var file in data.Files)
        {
            string filePath = Path.Combine(data.AbsoluteDirectoryPath, file.FileName);
            file.CRC = Crc32.ComputeFile(filePath);
        }

        foreach (var subDir in data.SubDirs)
        {
            CalculateCrc32(subDir);
        }
    }
}
