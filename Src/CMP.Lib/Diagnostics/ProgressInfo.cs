namespace CMP.Lib.Diagnostics;

/// <summary>
/// The information about current stage progress
/// </summary>
/// <param name="Phase"></param>
/// <param name="Current"></param>
/// <param name="Total"></param>
public sealed record ProgressInfo(
    string Phase,
    long Current,
    long? Total = null
);
