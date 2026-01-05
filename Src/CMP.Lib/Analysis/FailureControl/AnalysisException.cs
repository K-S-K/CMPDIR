namespace CMP.Lib.Analysis.FailureControl;

/// <summary>
/// The base exception class for all analysis failures types
/// </summary>
/// <param name="errorType">The type of failure</param>
/// <param name="ex">The original exception</param>
/// <param name="path">The path where the failure occurred</param>
internal class AnalysisException(ET errorType, Exception ex, string path) : Exception(ex.Message)
{
    public readonly ET ErrorType = errorType;
    public readonly string Path = path;

    public override string ToString()
    {
        return $"[{ErrorType}] at '{Path}': {Message}";
    }
}
