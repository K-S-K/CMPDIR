namespace CMP.Lib.Analysis;

/// <summary>
/// The comparison result enumeration
/// </summary>
/// <remarks>
/// This enumeration is used to represent the result of a comparison between
/// files and directories in the source and target locations.
/// We analyze the differences between paths, 
/// and file properties (size, CRC).
/// </remarks>
public enum CmpResult
{
    /// <summary>
    /// No changes detected between the file location and properties in source and target directories
    /// </summary>
    Equal,

    /// <summary>
    /// Added to the target directory (was not present in source but is present in target)
    /// </summary>
    Added,

    /// <summary>
    /// Deleted from the target directory (was present in source but not in target)
    /// </summary>
    Deleted,

    /// <summary>
    /// Moved or renamed in the target directory (path changed, but properties are the same)
    /// </summary>
    Moved,

    /// <summary>
    /// Modified in the target directory (properties changed and not moved)
    /// </summary>
    Modified,

    /// <summary>
    /// Duplicated in the target directory (same as Added but file or files with same properties also exist in source)
    /// </summary>
    Duplicated,

    /// <summary>
    /// Deduplicated in the target directory (same as Deleted but file or files with same properties also exist in target)
    /// </summary>
    Deduplicated,
}
