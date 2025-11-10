namespace CMP.Lib.Data;

/// <summary>
/// Single Directory Data Representation
/// </summary>
public class DirData
{
    public string DirPath { get; set; } = string.Empty;
    public string DirName => Path.GetFileName(DirPath.TrimEnd(Path.DirectorySeparatorChar));
    public List<FileData> Files { get; set; } = [];
    public List<DirData> SubDirs { get; set; } = [];
    public override string ToString()
    {
        return $"{DirName} (Files: {Files.Count}, SubDirs: {SubDirs.Count})";
    }
}
