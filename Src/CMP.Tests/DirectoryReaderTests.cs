using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

using CMP.Lib.Analysis;
using CMP.Lib.Data;

namespace CMP.Tests;

public class DirectoryReaderTests
{
    [Fact]
    public void DirReadTest()
    {
        string dirPath = @"/Users/ksk-work/Projects/CMPDIR/Dat/books.v2";

        DirData dirData = DirDataBuilder.BuildFromDirectory(dirPath);

        string jsonString = JsonSerializer.Serialize(
            dirData,
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            }
        );

        string ExportDirectory = @"/Users/ksk-work/Projects/CMPDIR/Dat/";
        string outputPath = Path.Combine(ExportDirectory, "books_v2_dirdata.json");
        File.WriteAllText(outputPath, jsonString);

        // Output the JSON string (for testing purposes)
        Console.WriteLine(jsonString);

    }
}
