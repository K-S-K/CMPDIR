namespace CMP.Lib.Data;

/// <summary>
/// Single Directory Data Representation
/// </summary>
public class DirData
{
    /// <summary>
    /// The absolute path of the directory
    /// </summary>
    public string AbsoluteDirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// The relative path of the directory
    /// relative to the root of the DirData tree
    /// </summary>
    public string RelativeDirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// The name of the directory
    /// </summary>
    public string DirName => Path.GetFileName(AbsoluteDirectoryPath.TrimEnd(Path.DirectorySeparatorChar));

    /// <summary>
    /// The list of files in the directory
    /// </summary>
    public List<FileData> Files { get; set; } = [];

    /// <summary>
    /// The list of subdirectories in the directory
    /// </summary>
    public List<DirData> SubDirs { get; set; } = [];

    /// <summary>
    /// Override ToString for better debugging output
    /// </summary>
    public override string ToString()
    {
        return $"{DirName} (Files: {Files.Count}, SubDirs: {SubDirs.Count})";
    }
}
