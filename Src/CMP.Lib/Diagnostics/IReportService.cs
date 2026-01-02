namespace CMP.Lib.Diagnostics;

public interface IReportService
{
    void Info(string message);
    void Warning(string message);
    void Error(string message);
}
