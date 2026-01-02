namespace CMP.Lib.Diagnostics;

/// <summary>
/// A report service that outputs to the console
/// </summary>
public sealed class ConsoleReportService : IReportService
{
    public void Info(string message)
        => Console.WriteLine(message);

    public void Warning(string message)
        => Console.WriteLine($"[WARN] {message}");

    public void Error(string message)
        => Console.Error.WriteLine($"[ERR ] {message}");
}
