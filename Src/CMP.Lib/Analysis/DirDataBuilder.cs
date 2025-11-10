using CMP.Lib.Data;

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
    /// <returns>The built DirData</returns>
    public static DirData BuildFromDirectory(string dirPath)
    {
        DirData dirData = new()
        {
            DirPath = dirPath
        };

        // Process files
        List<FileData> files = [];
        string[] fileEntries = Directory.GetFiles(dirPath);
        foreach (string filePath in fileEntries)
        {
            FileInfo fileInfo = new(filePath);
            FileData fileData = new()
            {
                FileName = Path.GetFileName(filePath),
                Size = fileInfo.Length,
                CRC = Crc32.ComputeFile(filePath),
            };
            files.Add(fileData);
        }
        dirData.Files = files.OrderBy(f => f.FileName).ToList();

        // Process subdirectories
        List<DirData> subDirs = [];
        string[] subDirEntries = Directory.GetDirectories(dirPath);
        foreach (string subDirPath in subDirEntries)
        {
            DirData subDirData = BuildFromDirectory(subDirPath);
            subDirs.Add(subDirData);
        }
        dirData.SubDirs = subDirs.OrderBy(d => d.DirName).ToList();

        return dirData;
    }
}
