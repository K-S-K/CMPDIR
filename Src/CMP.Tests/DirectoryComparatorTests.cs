using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;

using CMP.Lib.Data;
using CMP.Lib.Analysis;
using CMP.Lib.Diagnostics;

using CMP.Tests.Common;

namespace CMP.Tests;

public class DirectoryComparatorTests : TestBase
{
    [Fact]
    public void DirCompTest()
    {
        string dataDirectory = EnVar("CMPDIR_TEST_DATA_PATH");

        string sourceDirPath = Path.Combine(dataDirectory, "books.v1");
        string targetDirPath = Path.Combine(dataDirectory, "books.v2");

        IReportService reportService = new DiagnosticsReportService();
        IProgressReporter progressReporter = new DiagnosticsProgressReporter();

        DirData sourceDirData = new DirDataBuilder(reportService, progressReporter).BuildFromDirectory(sourceDirPath);
        DirData targetDirData = new DirDataBuilder(reportService, progressReporter).BuildFromDirectory(targetDirPath);

        Comparator.CompareDirData(sourceDirData, targetDirData);

        // Serialize results to JSON for inspection
        string sourceJsonActual = JsonSerializer.Serialize(
            sourceDirData,
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            }
        );

        // Serialize results to JSON for inspection
        string targetJsonActual = JsonSerializer.Serialize(
            targetDirData,
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            }
        );

        string sourceJsonExpected = GetResourceFileContent("CMP.Tests.Data.books_v1_compared.json");
        string targetJsonExpected = GetResourceFileContent("CMP.Tests.Data.books_v2_compared.json");

        sourceJsonActual = sourceJsonActual.Replace(dataDirectory, "...");
        sourceJsonExpected = sourceJsonExpected.Replace(dataDirectory, "...");

        targetJsonActual = targetJsonActual.Replace(dataDirectory, "...");
        targetJsonExpected = targetJsonExpected.Replace(dataDirectory, "...");

        Assert.True(
            CompareResult(
                sourceJsonActual, sourceJsonExpected,
                "CompareDirData",
                "books_v1_compared.json",
                out string sourcePath)
        , sourcePath);

        Assert.True(
            CompareResult(
                targetJsonActual, targetJsonExpected,
                "CompareDirData",
                "books_v2_compared.json",
                out string targetPath)
        , targetPath);
    }
}
