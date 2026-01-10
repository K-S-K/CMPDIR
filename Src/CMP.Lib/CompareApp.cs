using CMP.Lib.Analysis;
using CMP.Lib.Diagnostics;

namespace CMP.Lib;

public sealed class CompareApp
{
    private readonly DirectoryProcessor _dProcessor;
    private readonly FilePairProcessor _fProcessor;
    private readonly IReportService _report;

    public CompareApp(
        DirectoryProcessor dProcessor,
        FilePairProcessor fProcessor,
        IReportService report)
    {
        _dProcessor = dProcessor;
        _fProcessor = fProcessor;
        _report = report;
    }

    public int Run(string[] args)
    {
        if (args.Length == 3)
            return CompareDirectories(args);

        if (args.Length == 2)
            return AnalyzeDirectoryContent(args);

        if (args.Length == 1)
            return CompareFilePairs(args);

        PrintUsage();
        return 1;
    }

    private int CompareDirectories(string[] args)
    {
        string source = args[0];
        string target = args[1];
        string output = args[2];

        if (_dProcessor.CompareDirectoriesContents(source, target, out string? json))
        {
            File.WriteAllText(output, json);
            _report.Info("Comparison completed successfully.");
            return 0;
        }

        _report.Error("Failed to compare directories.");
        _report.Error(json);
        return 2;
    }

    private int AnalyzeDirectoryContent(string[] args)
    {
        string dir = args[0];
        string outputFilePath = args[1];

        if (_dProcessor.BuildDirectoryContent(dir, out string? json))
        {
            File.WriteAllText(outputFilePath, json);
            _report.Info($"Directory content saved to: {outputFilePath}");
            return 0;
        }

        _report.Error($"Failed to build directory content for: {dir}");
        _report.Error(json);
        return 2;
    }

    private int CompareFilePairs(string[] args)
    {
        if(_fProcessor.CompareFilePair(args[0], out string? report))
        {
            _report.Info("File pairs comparison completed successfully.");
            _report.Info(report ?? "No report available");
            return 0;
        }

        _report.Error("Failed to compare file pairs.");
        _report.Error(report ?? "No error details available");
        return 2;
    }

    private void PrintUsage()
    {
        _report.Info("Usage:");
        _report.Info("  To list directory content: CMP.Cmd <dirPath> <outputFilePath>");
        _report.Info("  To compare directories: CMP.Cmd <sourceDirPath> <targetDirPath> <reportFilePath>");
    }
}
