namespace CMP.Lib.Data;

/// <summary>
/// Single File Data Representation
/// </summary>
public class FileData
{
    /// <summary>
    /// The name of the file
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The relative path of the file
    /// </summary>
    public string RelativeDirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// The size of the file
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// The CRC32 checksum of the file
    /// </summary>
    public uint CRC { get; set; }

    /// <summary>
    /// The comparison result of the file
    /// </summary>
    public FileCmpResult? CmpResult { get; set; }

    /// <summary>
    /// The property index value for quick comparison
    /// </summary>
    public string PropertyIndexValue => $"{Size}-{CRC:X8}";

    /// <summary>
    /// Override ToString for better debugging output
    /// </summary>
    public override string ToString()
    {
        return $"[{RelativeDirectoryPath}] {FileName} CMP: {{{(CmpResult==null?"":CmpResult.Result)}}} (Size: {Size}, CRC: {CRC:X8})";
    }
}
