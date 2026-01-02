namespace CMP.Lib.Diagnostics;

public sealed class ConsoleProgressReporter : IProgressReporter
{
    private readonly ConsoleLineUpdater _updater = new();

    public void Report(ProgressInfo info)
    {
        string text = info.Total is null
            ? $"{info.Phase}... {info.Current:N0}"
            : $"{info.Phase}... {info.Current * 100.0 / info.Total:0.0}%";

        _updater.Update(text);
    }
}
