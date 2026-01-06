namespace CMP.Lib.Analysis.FailureControl;

/// <summary>
/// The exception class for directory listing failures
/// </summary>
/// <param name="ex">The original exception</param>
/// <param name="path">The path where the failure occurred</param>
internal class DirectoryListingException(Exception ex, string path)
    : AnalysisException(ET.DirsList, ex, path)
{
}
