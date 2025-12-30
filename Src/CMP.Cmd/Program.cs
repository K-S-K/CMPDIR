using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;

using CMP.Lib.Data;
using CMP.Lib.Analysis;

internal class Program
{
    private static void Main(string[] args)
    {
        // Write greeting message to the console
        Console.WriteLine("Directory Comparator Program");
        if (args.Length == 3)
        {
            string sourceDirPath = args[0];
            string targetDirPath = args[1];
            string reportFilePath = args[2];

            if (CompareDirectoriesContents(sourceDirPath, targetDirPath, out string resultJson))
            {
                System.IO.File.WriteAllText(reportFilePath, resultJson);
            }
            else
            {
                Console.WriteLine("Failed to compare directories.");
                Console.WriteLine(resultJson);
            }
        }
        else if (args.Length == 2)
        {
            string dirPath = args[0];
            string outputFilePath = args[1];

            if (BuildDirectoryContent(dirPath, out string jsonString))
            {
                System.IO.File.WriteAllText(outputFilePath, jsonString);
            }
            else
            {
                Console.WriteLine("Failed to build directory content.");
                Console.WriteLine(jsonString);
            }
        }
        else
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  To list directory content: CMP.Cmd <dirPath> <outputFilePath>");
            Console.WriteLine("  To compare directories: CMP.Cmd <sourceDirPath> <targetDirPath> <reportFilePath>");
        }
    }

    private static bool CompareDirectoriesContents(string sourceDirPath, string targetDirPath, out string resultJson)
    {
        try
        {
            DirData sourceDirData = DirDataBuilder.BuildFromDirectory(sourceDirPath);
            DirData targetDirData = DirDataBuilder.BuildFromDirectory(targetDirPath);

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

    private static bool BuildDirectoryContent(string dirPath, out string jsonString)
    {
        try
        {
            DirData dirData = DirDataBuilder.BuildFromDirectory(dirPath);

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
}
