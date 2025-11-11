using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using CMP.Lib.Analysis;
using CMP.Lib.Data;

namespace CMP.Tests;

public class DirectoryComparatorTests
{
    [Fact]
    public void DirCompTest()
    {
        string sourceDirPath = @"/Users/ksk-work/Projects/CMPDIR/Dat/books.v1";
        string targetDirPath = @"/Users/ksk-work/Projects/CMPDIR/Dat/books.v2";

        DirData sourceDirData = DirDataBuilder.BuildFromDirectory(sourceDirPath);
        DirData targetDirData = DirDataBuilder.BuildFromDirectory(targetDirPath);

        Comparator.CompareDirData(sourceDirData, targetDirData);

        // Serialize results to JSON for inspection
        string sourceJson = JsonSerializer.Serialize(
            sourceDirData,
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            }
        );

        string targetJson = JsonSerializer.Serialize(
            targetDirData,
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            }
        );

        string ExportDirectory = @"/Users/ksk-work/Projects/CMPDIR/Dat/";
        
        string sourceOutputPath = Path.Combine(ExportDirectory, "books_v1_compared.json");
        File.WriteAllText(sourceOutputPath, sourceJson);

        string targetOutputPath = Path.Combine(ExportDirectory, "books_v2_compared.json");
        File.WriteAllText(targetOutputPath, targetJson);
    }
}
