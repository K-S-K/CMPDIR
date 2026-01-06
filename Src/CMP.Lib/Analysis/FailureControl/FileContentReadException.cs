namespace CMP.Lib.Analysis.FailureControl;

/// <summary>
/// The exception class for file content reading failures
/// </summary>
/// <param name="ex">The original exception</param>
/// <param name="path">The path where the failure occurred</param>
internal class FileContentReadException(Exception ex, string path)
    : AnalysisException(ET.FileRead, ex, path)
{
}
