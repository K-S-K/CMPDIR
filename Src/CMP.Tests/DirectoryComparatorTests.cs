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

        IReportService reportService = new ReportServiceStub();
        IProgressReporter progressReporter = new ProgressReporterStub();

        DirData sourceDirData = new DirDataBuilder(reportService, progressReporter).BuildFromDirectory(sourceDirPath, out _);
        DirData targetDirData = new DirDataBuilder(reportService, progressReporter).BuildFromDirectory(targetDirPath, out _);

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


        bool compareResult1 = CompareResult(
                sourceJsonActual, sourceJsonExpected,
                "CompareDirData",
                "books_v1_compared.json",
                out string cmpResultPath1);

        bool compareResult2 = CompareResult(
                targetJsonActual, targetJsonExpected,
                "CompareDirData",
                "books_v2_compared.json",
                out string cmpResultPath2);

        Assert.True(compareResult2, cmpResultPath2);
        Assert.True(compareResult1, cmpResultPath1);
    }
}
