using CMP.Lib.Analysis;
using CMP.Lib.Diagnostics;

using CMP.Tests.Common;

namespace CMP.Tests;

public class FilePairTests : TestBase
{
    [Fact]
    public void FilePairCompTest()
    {
        string dataDirectory = TestSubDir("FilePairs");
        string filePairJsonPathIn = Path.Combine(dataDirectory, "file_pairs.json");
        string filePairJsonPathOut = Path.Combine(dataDirectory, "file_pairs_out.json");

        // Save test input file
        string filePairJsonIn = GetResourceFileContent("CMP.Tests.Data.file_pairs.json");
        File.WriteAllText(filePairJsonPathIn, filePairJsonIn);

        // Prepare FilePairProcessor
        FilePairProcessor filePairProcessor = new(
            new ReportServiceStub(),
            new ProgressReporterStub()
        );

        // Execute file pair comparison
        bool success = filePairProcessor.CompareFilePair(filePairJsonPathIn, out string report);

        // Verify success
        Assert.True(success);

        // Load expected and actual reports
        string reportExpected = GetResourceFileContent("CMP.Tests.Data.file_pairs_out.json");
        string reportActual = File.ReadAllText(filePairJsonPathOut);
        Assert.Equal(reportExpected, reportActual);
    }
}
