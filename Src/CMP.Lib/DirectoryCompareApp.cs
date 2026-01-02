using CMP.Lib.Rpt;
using CMP.Lib.Analysis;

namespace CMP.Lib;

public sealed class DirectoryCompareApp
{
    private readonly DirectoryProcessor _processor;
    private readonly IReportService _report;

    public DirectoryCompareApp(
        DirectoryProcessor processor,
        IReportService report)
    {
        _processor = processor;
        _report = report;
    }

    public int Run(string[] args)
    {
        if (args.Length == 3)
            return Compare(args);

        if (args.Length == 2)
            return Build(args);

        PrintUsage();
        return 1;
    }

    private int Compare(string[] args)
    {
        var source = args[0];
        var target = args[1];
        var output = args[2];

        if (_processor.CompareDirectoriesContents(source, target, out var json))
        {
            File.WriteAllText(output, json);
            _report.Info("Comparison completed successfully.");
            return 0;
        }

        _report.Error("Failed to compare directories.");
        _report.Error(json);
        return 2;
    }

    private int Build(string[] args)
    {
        var dir = args[0];
        var outputFilePath = args[1];

        if (_processor.BuildDirectoryContent(dir, out var json))
        {
            File.WriteAllText(outputFilePath, json);
            _report.Info($"Directory content saved to: {outputFilePath}");
            return 0;
        }

        _report.Error($"Failed to build directory content for: {dir}");
        _report.Error(json);
        return 2;
    }

    private void PrintUsage()
    {
        _report.Info("Usage:");
        _report.Info("  To list directory content: CMP.Cmd <dirPath> <outputFilePath>");
        _report.Info("  To compare directories: CMP.Cmd <sourceDirPath> <targetDirPath> <reportFilePath>");
    }
}
