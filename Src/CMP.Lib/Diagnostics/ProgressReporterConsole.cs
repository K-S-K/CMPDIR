using CMP.Lib.Data;
using System.Diagnostics;

namespace CMP.Lib.Diagnostics;

public sealed class ProgressReporterConsole : IProgressReporter
{
    private readonly ConsoleLineUpdater _updater = new();
    private readonly Stopwatch _stopwatch = new();
    private FileData? _lastReportedFile = null;

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
                longProcessingFileName = $"    Processing file: {info.File.FileName}";
            }
        }
        else
        {
            _lastReportedFile = null;
        }

        string text = info.TotalCount is null || info.TotalSize is null
            ? $"{info.Phase}...    {info.CurrentCount:N0} files, {SizeWithSuffix(info.CurrentSize)}"
            : $"{info.Phase}    {info.CurrentCount} of {info.TotalCount} files, {PercentageByFiles(info)},    {SizeWithSuffix(info.CurrentSize)} of {SizeWithSuffix(info.TotalSize ?? 0)} data, {PercentageBySize(info)},";

        // Remember current color and set to yellow
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;

        _updater.Update($"{text}    elapsed time: {DurationToString(info.Elapsed)}{longProcessingFileName}");

        // Restore original color
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// Clears the progress report from the console.
    /// </summary>
    public void Clear() => _updater.Clear();

    /// <summary>
    /// Returns a string representing the percentage of completion based on file counts.
    /// </summary>
    /// <param name="info">The progress information containing current and total counts.</param>
    /// <returns>A string representing the percentage of completion.</returns>
    private string PercentageByFiles(ProgressInfo info)
    {
        string result;

        try
        {
            result = Percentage(info.CurrentCount, info.TotalCount ?? 0);
        }
        catch (Exception)
        {
            result = "N/A";
        }

        return result;
    }

    /// <summary>
    /// Returns a string representing the percentage of completion based on file sizes.
    /// </summary>
    /// <param name="info">The progress information containing current and total sizes.</param>
    /// <returns>A string representing the percentage of completion.</returns>
    private string PercentageBySize(ProgressInfo info)
    {
        string result;
        try
        {
            result = Percentage(info.CurrentSize, info.TotalSize ?? 0);
        }
        catch (Exception)
        {
            result = "N/A";
        }

        return result;
    }

    /// <summary>
    /// Returns a string representing the percentage of completion.
    /// </summary>
    /// <param name="current">The current value.</param>
    /// <param name="total">The total value.</param>
    /// <returns>A string representing the percentage of completion.</returns>
    private static string Percentage(long current, long total)
    {
        if (total == 0) return "0.0%";
        return $"{current * 100.0 / total:0.0}%";
    }

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

    /// <summary>
    /// Returns a string representation of a TimeSpan duration including milliseconds.
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Returns a string representation of a TimeSpan duration without milliseconds.
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
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
