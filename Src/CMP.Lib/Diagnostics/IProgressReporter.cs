namespace CMP.Lib.Diagnostics;

/// <summary>
/// The interface for progress displaying
/// </summary>
public interface IProgressReporter
{
    void Report(ProgressInfo info);
}
