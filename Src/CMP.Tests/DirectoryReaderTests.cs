using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;

using CMP.Lib.Data;
using CMP.Lib.Analysis;
using CMP.Lib.Diagnostics;

using CMP.Tests.Common;

namespace CMP.Tests;

public class DirectoryReaderTests : TestBase
{
    [Fact]
    public void DirReadTest()
    {
        IReportService reportService = new DiagnosticsReportService();
        string dataDirectory = EnVar("CMPDIR_TEST_DATA_PATH");
        string dirPath = Path.Combine(dataDirectory, "books.v2");

        DirData dirData = DirDataBuilder.BuildFromDirectory(dirPath, TNL.Root, reportService);
        string jsonStringActual = JsonSerializer.Serialize(
            dirData,
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            }
        );

        string jsonStringExpected = GetResourceFileContent("CMP.Tests.Data.books_v2_directory_data.json");

        // jsonStringActual = jsonStringActual.Replace(dataDirectory.Replace("\\", "\\\\"), "...");
        // jsonStringExpected = jsonStringExpected.Replace(dataDirectory.Replace("\\", "/"), "...");

        bool result = CompareResult(
                jsonStringActual, jsonStringExpected,
                "BuildFromDirectory",
                "books_v2_directory_data.json",
                out string path);
        Assert.True(result, path ?? "");

        // Output the JSON string (for testing purposes)
        Console.WriteLine(jsonStringActual);
    }
}
