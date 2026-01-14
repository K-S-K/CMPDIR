using CMP.Lib.Analysis;
using System.Text.Json.Serialization;

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
    /// The full path of the file
    /// </summary>
    [JsonIgnore]
    public string FullPath => Path.Combine(RelativeDirectoryPath, FileName);

    /// <summary>
    /// The analysis stage of the file
    /// </summary>
    // add attribute to serialize as string in json
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AnalysisStage AnalysisStage { get; set; } = AnalysisStage.Listed;

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
        return $"CMP: {{{(CmpResult == null ? "" : CmpResult.Result)}}} [{RelativeDirectoryPath}] {FileName} (Size: {Size}, CRC: {CRC:X8})";
    }
}
