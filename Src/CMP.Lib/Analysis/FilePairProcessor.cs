using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;

using CMP.Lib.Data;
using CMP.Lib.Diagnostics;

namespace CMP.Lib.Analysis;

public class FilePairProcessor
{
    private readonly IReportService _reportService;
    private readonly IProgressReporter _progressReporter;

    public FilePairProcessor(IReportService reportService, IProgressReporter progressReporter)
    {
        _reportService = reportService;
        _progressReporter = progressReporter;
    }


    public bool CompareFilePair(string filePairJsonPath, out string report)
    {
        // Check if input file exists
        if (!File.Exists(filePairJsonPath))
        {
            report = $"Error: Input file not found - {filePairJsonPath}";
            return false;
        }

        // Deserialize input FilePairData
        string filePairJsonIn;

        try
        {
            filePairJsonIn = File.ReadAllText(filePairJsonPath);
        }
        catch (Exception ex)
        {
            report = $"Error: Failed to read input file - {ex.Message}";
            return false;
        }

        // Deserialize file pairs
        List<FilePairData> filePairData = [];
        try
        {
            filePairData = JsonSerializer.Deserialize<List<FilePairData>>(filePairJsonIn) ?? [];
            _reportService.Info($"Successfully read input file with {filePairData.Count} pair(s)");
        }
        catch (Exception ex)
        {
            report = $"Error: Failed to deserialize input JSON - {ex.Message}";
            return false;
        }

        // Compare each file pair
        List<FileCmpResult> cmpResult = [];
        foreach (FilePairData filePair in filePairData)
        {
            FileCmpResult result = CompareFilePair(filePair);
            cmpResult.Add(result);
        }

        string filePairJsonOut = JsonSerializer.Serialize(
            cmpResult,
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            }
        );

        // Prepare output file path
        string filePairJsonPathWithoutExt = Path.Combine(
            Path.GetDirectoryName(filePairJsonPath) ?? "",
            Path.GetFileNameWithoutExtension(filePairJsonPath)
        );
        string outputFilePath = $"{filePairJsonPathWithoutExt}_out.json";

        // Write output JSON to file
        try
        {
            File.WriteAllText(outputFilePath, filePairJsonOut);
            report = $"Comparison completed successfully. Output saved to: {outputFilePath}";
        }
        catch (Exception ex)
        {
            report = $"Error: Failed to write output file - {ex.Message}";
            return false;
        }

        return true;
    }

    private FileCmpResult CompareFilePair(FilePairData filePairData)
    {
        // Prepare FileData objects for both files
        FileData fdA = new()
        {
            RelativeDirectoryPath = Path.GetDirectoryName(filePairData.A) ?? "",
            FileName = Path.GetFileName(filePairData.A),
        };
        FileData fdB = new()
        {
            RelativeDirectoryPath = Path.GetDirectoryName(filePairData.B) ?? "",
            FileName = Path.GetFileName(filePairData.B),
        };

        FileCmpResult result = new()
        {
            ForPairs = true,
            Files = [fdA, fdB]
        };

        #region -> Try to read file info for the existing files
        {
            if (File.Exists(filePairData.A))
            {
                try
                {
                    FileInfo fiA = new(filePairData.A);
                    fdA.Size = fiA.Length;
                    fdA.AnalysisStage = AnalysisStage.Listed;
                    fdA.RelativeDirectoryPath = fiA.DirectoryName ?? "";
                    fdA.FileName = fiA.Name;
                }
                catch (Exception)
                {
                    fdA.Size = 0;
                    fdA.AnalysisStage = AnalysisStage.FailedOnListing;
                }
            }
            else
            {
                fdA.CRC = 0;
                fdA.Size = 0;
                fdA.AnalysisStage = AnalysisStage.Absent;
            }

            if (File.Exists(filePairData.B))
            {
                try
                {
                    FileInfo fiB = new(filePairData.B);
                    fdB.Size = fiB.Length;
                    fdB.AnalysisStage = AnalysisStage.Listed;
                    fdB.RelativeDirectoryPath = fiB.DirectoryName ?? "";
                    fdB.FileName = fiB.Name;
                }
                catch (Exception)
                {
                    fdB.Size = 0;
                    fdB.AnalysisStage = AnalysisStage.FailedOnListing;
                }
            }
            else
            {
                fdB.CRC = 0;
                fdB.Size = 0;
                fdB.AnalysisStage = AnalysisStage.Absent;
            }
        }
        #endregion

        #region -> Try to compute CRC32 for the listed files
        {
            if (fdA.AnalysisStage == AnalysisStage.Listed)
            {
                try
                {
                    fdA.CRC = Crc32.ComputeFile(filePairData.A);
                    fdA.AnalysisStage |= AnalysisStage.Measured;
                }
                catch (Exception)
                {
                    fdA.CRC = 0;
                    fdA.AnalysisStage |= AnalysisStage.FailedOnMeasuring;
                }
            }

            if (fdB.AnalysisStage == AnalysisStage.Listed)
            {
                try
                {
                    fdB.CRC = Crc32.ComputeFile(filePairData.B);
                    fdB.AnalysisStage |= AnalysisStage.Measured;
                }
                catch (Exception)
                {
                    fdB.CRC = 0;
                    fdB.AnalysisStage |= AnalysisStage.FailedOnMeasuring;
                }
            }
        }
        #endregion

        // Compare the two FileData objects
        if (
            fdA.AnalysisStage == AnalysisStage.Absent &&
            fdB.AnalysisStage == AnalysisStage.Absent
            )
        {
            result.Result = CmpResult.Error;
        }
        else if (fdA.AnalysisStage == AnalysisStage.Absent)
        {
            result.Result = CmpResult.Added;
        }
        else if (fdB.AnalysisStage == AnalysisStage.Absent)
        {
            result.Result = CmpResult.Deleted;
        }
        else if (
                !fdA.AnalysisStage.HasFlag(AnalysisStage.Measured) ||
                !fdB.AnalysisStage.HasFlag(AnalysisStage.Measured)
                )
        {
            result.Result = CmpResult.Error;
        }
        else if (fdA.Size != fdB.Size)
        {
            result.Result = CmpResult.Modified;
        }
        else if (fdA.CRC != fdB.CRC)
        {
            result.Result = CmpResult.Modified;
        }
        else
        {
            result.Result = CmpResult.Equal;
        }


        return result;
    }
}
