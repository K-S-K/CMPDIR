using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;

using CMP.Lib.Data;
using CMP.Lib.Diagnostics;

namespace CMP.Lib.Analysis;

public class DirectoryProcessor
{
    private readonly IReportService _reportService;
    private readonly IProgressReporter _progressReporter;

    public DirectoryProcessor(IReportService reportService, IProgressReporter progressReporter)
    {
        _reportService = reportService;
        _progressReporter = progressReporter;
    }

    public bool BuildDirectoryContent(string dirPath, out string jsonString)
    {
        try
        {
            DirData dirData = new DirDataBuilder(_reportService, _progressReporter).BuildFromDirectory(dirPath, out _);

            jsonString = JsonSerializer.Serialize(
                dirData,
                new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = true
                }
            );
        }
        catch (Exception ex)
        {
            jsonString = $"Error: {ex.Message}";
            return false;
        }

        return true;
    }

    public bool CompareDirectoriesContents(string sourceDirPath, string targetDirPath, out string resultJson)
    {
        try
        {
            DirData sourceDirData = new DirDataBuilder(_reportService, _progressReporter).BuildFromDirectory(sourceDirPath, out IEnumerable<string> errS);
            DirData targetDirData = new DirDataBuilder(_reportService, _progressReporter).BuildFromDirectory(targetDirPath, out IEnumerable<string> errT);

            Comparator.CompareDirData(sourceDirData, targetDirData);

            var comparisonResult = new
            {
                Source = sourceDirData,
                Target = targetDirData,
                Errors = errS.ToList().Concat(errT).ToList()
            };

            resultJson = JsonSerializer.Serialize(
                comparisonResult,
                new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = true
                }
            );
        }
        catch (Exception ex)
        {
            resultJson = $"Error: {ex.Message}";
            return false;
        }

        return true;
    }
}
