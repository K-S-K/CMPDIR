namespace CMP.Lib.Diagnostics;

/// <summary>
/// A report service that outputs to the console
/// </summary>
public sealed class ReportServiceConsole : IReportService
{
    public void Info(string message)
        => Console.WriteLine(message);

    public void Warning(string message)
    {
        // Remember current color and set to yellow
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine(message);

        // Restore original color
        Console.ForegroundColor = originalColor;
    }

    public void Error(string message)
    {
        // Remember current color and set to red
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Error.WriteLine(message);

        // Restore original color
        Console.ForegroundColor = originalColor;
    }
}
