namespace CMP.Lib.Data;

/// <summary>
/// Single File Data Representation
/// </summary>
public class FileData
{
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public uint CRC { get; set; }

    public override string ToString()
    {
        return $"{FileName} (Size: {Size}, CRC: {CRC:X8})";
    }
}
