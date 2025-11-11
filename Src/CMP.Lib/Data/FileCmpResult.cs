using System.Text.Json.Serialization;
using CMP.Lib.Analysis;

namespace CMP.Lib.Data;

/// <summary>
/// File Comparison Result Representation
/// </summary>
/// <remarks>
/// This class represents the result of comparing a given file in source/target directory
/// with file(s) in the target/source directory. It includes the comparison result (CmpResult)
/// and absolute paths to the related source or target FileData objects depending on whether it belongs to source or target:
/// if the given file is in source directory, then reference related files in target;
/// if the given file is in target directory, then reference related files in source.
/// </remarks>
public class FileCmpResult
{
    /// <summary>
    /// The comparison result enum value
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CmpResult Result { get; set; } = CmpResult.Equal;

    /// <summary>
    /// The list of related FileData objects in the other directory
    /// </summary>
    [JsonIgnore]
    public List<FileData> Files { get; set; } = [];

    /// <summary>
    /// The list of related file paths in the other directory
    /// </summary>
    public List<string> FilePaths => Files.Select(f => f.FileName).ToList();

    /// <summary>
    /// Override ToString for better debugging output
    /// </summary>
    public override string ToString()
    {
        return $"{Result} Count: {Files.Count}";
    }
}
