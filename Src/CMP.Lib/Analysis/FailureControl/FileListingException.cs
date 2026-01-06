namespace CMP.Lib.Analysis.FailureControl;

/// <summary>
/// The exception class for file listing failures
/// </summary>
/// <param name="ex">The original exception</param>
/// <param name="path">The path where the failure occurred</param>
internal class FileListingException(Exception ex, string path)
    : AnalysisException(ET.FileList, ex, path)
{
}
