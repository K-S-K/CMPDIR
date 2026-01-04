namespace CMP.Lib.Diagnostics;

/// <summary>
/// The information about current stage progress
/// </summary>
/// <param name="Phase"></param>
/// <param name="Elapsed"></param>
/// <param name="CurrentCount"></param>
/// <param name="TotalCount"></param>
/// <param name="CurrentSize"></param>
/// <param name="TotalSize"></param>
public sealed record ProgressInfo(
    string Phase,
    TimeSpan Elapsed,
    long CurrentCount,
    long? TotalCount,
    long CurrentSize,
    long? TotalSize
);
