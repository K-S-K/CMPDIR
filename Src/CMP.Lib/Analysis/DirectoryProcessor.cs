using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;

using CMP.Lib.Data;
using CMP.Lib.Diagnostics;

namespace CMP.Lib.Analysis;

public class DirectoryProcessor
{
    private readonly IReportService _reportService;

    public DirectoryProcessor(IReportService reportService)
    {
        _reportService = reportService;
    }

    public bool BuildDirectoryContent(string dirPath, out string jsonString)
    {
        try
        {
            DirData dirData = DirDataBuilder.BuildFromDirectory(dirPath, TNL.Root, _reportService);

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
            DirData sourceDirData = DirDataBuilder.BuildFromDirectory(sourceDirPath, TNL.Root, _reportService);
            DirData targetDirData = DirDataBuilder.BuildFromDirectory(targetDirPath, TNL.Root, _reportService);

            Comparator.CompareDirData(sourceDirData, targetDirData);

            var comparisonResult = new
            {
                Source = sourceDirData,
                Target = targetDirData
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
