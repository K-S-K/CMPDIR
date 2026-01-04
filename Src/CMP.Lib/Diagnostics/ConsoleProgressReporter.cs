using CMP.Lib.Data;
using System.Diagnostics;

namespace CMP.Lib.Diagnostics;

public sealed class ConsoleProgressReporter : IProgressReporter
{
    private readonly ConsoleLineUpdater _updater = new();
    private FileData? _lastReportedFile = null;
    private Stopwatch _stopwatch = new();

    public void Report(ProgressInfo info)
    {
        string longProcessingFileName = string.Empty;

        // If the current file if handled for more than 5 seconds, print its name
        if (info.File != null)
        {
            if (_lastReportedFile != info.File)
            {
                _stopwatch.Restart();
                _lastReportedFile = info.File;
            }
            else if (_stopwatch.Elapsed.TotalSeconds >= 5)
            {
                longProcessingFileName=$"    Processing file: {info.File.FileName}";
            }
        }
        else
        {
            _lastReportedFile = null;
        }

        string text = info.TotalCount is null || info.TotalSize is null
            ? $"{info.Phase}...    {info.CurrentCount:N0} files, {SizeWithSuffix(info.CurrentSize)}"
            : $"{info.Phase} {info.CurrentCount * 100.0 / info.TotalCount:0.0}%,    {info.CurrentCount} of {info.TotalCount} files,    {SizeWithSuffix(info.CurrentSize)} of {SizeWithSuffix(info.TotalSize ?? 0)} data,";

        // Remember current color and set to yellow
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;

        _updater.Update($"{text}    elapsed time: {DurationToString(info.Elapsed)}{longProcessingFileName}");

        // Restore original color
        Console.ForegroundColor = originalColor;
    }

    public void Clear() => _updater.Clear();

    /// <summary>
    /// Returns a human-readable string representation of a file size.
    /// </summary>
    /// <param name="size">The size in bytes.</param>
    /// <returns>A string representing the size in appropriate units (B, KB, MB, GB).</returns>
    public static string SizeWithSuffix(long size)
    {
        if (size >= 1024 * 1024 * 1024)
            return $"{size / 1024 / 1024 / 1024.0:0.00} GB";
        else if (size >= 1024 * 1024)
            return $"{size / 1024 / 1024.0:0.00} MB";
        else if (size >= 1024)
            return $"{size / 1024.0:0.00} KB";
        else
            return $"{size} B";
    }

    public static string DurationToStringMs(TimeSpan duration)
    {
        string result = $"{duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00}.{(int)duration.Milliseconds:000}";
        result = result.TrimStart("00:".ToCharArray());

        if (duration.TotalSeconds < 1)
        {
            result = "0" + result;
        }

        if (duration.TotalMinutes < 1)
        {
            result += " s";
        }

        return result;
    }

    public static string DurationToString(TimeSpan duration)
    {
        string result = $"{duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
        result = result.TrimStart("00:".ToCharArray());

        if (duration.TotalMinutes < 1)
        {
            result += " s";
        }

        return result;
    }
}
