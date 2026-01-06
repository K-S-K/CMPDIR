namespace CMP.Lib.Analysis.FailureControl
{
    /// <summary>
    /// The enumeration of failure handling strategies
    /// </summary>
    public enum ET
    {
        /// <summary>
        /// Undefined failure type
        /// never use this value
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Failure during directory listing
        /// </summary>
        DirsList,

        /// <summary>
        /// Failure during file listing
        /// </summary>
        FileList,

        /// <summary>
        /// Failure during reading file metadata
        /// </summary>
        FileHead,

        /// <summary>
        /// Failure during reading file content
        /// </summary>
        FileRead,
    }
}
