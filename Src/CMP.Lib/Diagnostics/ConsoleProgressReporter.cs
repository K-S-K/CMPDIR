namespace CMP.Lib.Diagnostics;

public sealed class ConsoleProgressReporter : IProgressReporter
{
    private readonly ConsoleLineUpdater _updater = new();

    public void Report(ProgressInfo info)
    {
        string text = info.Total is null
            ? $"{info.Phase}...    {info.Current:N0}"
            : $"{info.Phase}...     {info.Current} of {info.Total}, progress {info.Current * 100.0 / info.Total:0.0}%";

        // Remember current color and set to yellow
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;

        _updater.Update($"{text}    elapsed time {info.Elapsed.Hours:00}:{info.Elapsed.Minutes:00}:{info.Elapsed.Seconds:00}");

        // Restore original color
        Console.ForegroundColor = originalColor;
    }

    public void Clear() => _updater.Clear();
}
