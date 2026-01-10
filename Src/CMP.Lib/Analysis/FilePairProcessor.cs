using CMP.Lib.Data;
using CMP.Lib.Diagnostics;

namespace CMP.Lib.Analysis;

public class FilePairProcessor
{
    private readonly IReportService _reportService;
    private readonly IProgressReporter _progressReporter;

    public FilePairProcessor(IReportService reportService, IProgressReporter progressReporter)
    {
        _reportService = reportService;
        _progressReporter = progressReporter;
    }


    public bool CompareFilePair(string filePairJsonIn, out string? filePairJsonOut)
    {
        filePairJsonOut = null;
        
        return true;
    }

    private FileCmpResult CompareFilePair(FilePairData filePairData)
    {
        throw new NotImplementedException();
    }
}
