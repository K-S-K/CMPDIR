using System.Text.Json.Serialization;

namespace CMP.Lib.Analysis;

/// <summary>
/// Represents a pair of file paths for comparison.
/// </summary>
public class FilePairData
{
    [JsonPropertyName("a")]
    public string A { get; set; } = null!;
    [JsonPropertyName("b")]
    public string B { get; set; } = null!;
}
