namespace CMP.Lib.Analysis;

/// <summary>
/// The analysis stage enumeration
/// </summary>
[Flags]
public enum AnalysisStage
{
    /// <summary>
    /// Files are being listed and size if the file was retrieved from the file system
    /// </summary>
    Listed = 1,

    /// <summary>
    /// Failed to get the information about the size of the file during listing
    /// </summary>
    FailedOnListing = 2,

    /// <summary>
    /// The CRC32 checksum of the file is being measured
    /// </summary>
    Measured = 4,

    /// <summary>
    /// Failed to measure the CRC32 checksum of the file
    /// </summary>
    FailedOnMeasuring = 8,
}
