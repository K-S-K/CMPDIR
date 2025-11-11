namespace CMP.Lib.Data;

/// <summary>
/// File Index Representation
/// </summary>
public class FileIndex
{
    #region -> Data
    /// <summary>
    /// The File Index Dictionary by relative file paths
    /// </summary>
    private Dictionary<string, FileData> _pathIndex { get; set; } = [];

    /// <summary>
    /// The Property Index Dictionary by property index values
    /// </summary>
    private Dictionary<string, List<FileData>> _propertyIndex { get; set; } = [];
    #endregion


    #region -> Properties
    /// <summary>
    /// The root directory path of the indexed files
    /// </summary>
    public string RootDirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// The list of all indexed files
    /// </summary>
    public List<FileData> Files => _pathIndex.Values.ToList();

    /// <summary>
    /// The list of all property index values
    /// </summary>
    public List<string> PropertyIndexes => _propertyIndex.Keys.ToList();
    #endregion


    #region -> Methods
    /// <summary>
    /// Try to get FileData by relative file path
    /// </summary>
    /// <param name="relativePath">The relative file path</param>
    /// <param name="fileData">The output FileData if found</param>
    /// <returns>True if found, false otherwise</returns>
    public bool TryGetFileData(string relativePath, out FileData? fileData)
    {
        return _pathIndex.TryGetValue(relativePath, out fileData);
    }

    /// <summary>
    /// Try to get FileData by property index value
    /// </summary>
    /// <param name="propertyIndexValue">The property index value</param>
    /// <param name="fileData">The output FileData if found</param>
    /// <returns>True if found, false otherwise</returns>
    public bool TryGetFileDataByPropertyIndex(string propertyIndexValue, out List<FileData> fileData)
    {
        if (!_propertyIndex.TryGetValue(propertyIndexValue, out List<FileData>? result))
        {
            result = [];
        }
        fileData = result;
        return fileData.Count > 0;
    }

    /// <summary>
    /// Override ToString for better debugging output
    /// </summary>
    public override string ToString()
    {
        return $"{RootDirectoryPath}: File count: {_pathIndex.Count}";
    }
    #endregion


    #region -> Constructors
    /// <summary>
    /// Create FileIndex from DirData
    /// </summary>
    /// <param name="dirData">The DirData to create from</param>
    /// <returns>The created FileIndex</returns>
    public static FileIndex FromDirData(DirData dirData)
    {
        FileIndex fileIndex = new();

        // Get the last element in the directory path
        fileIndex.RootDirectoryPath = Path.GetFileName(dirData.AbsoluteDirectoryPath.TrimEnd(Path.DirectorySeparatorChar));

        BuildFileIndexRecursive(dirData, fileIndex);
        return fileIndex;
    }
    #endregion


    #region -> Implementation
    /// <summary>
    /// Build the file index recursively
    /// </summary>
    /// <param name="dirData">The directory data to index</param>
    /// <param name="fileIndex">The file index to populate</param>
    private static void BuildFileIndexRecursive(DirData dirData, FileIndex fileIndex)
    {
        foreach (var fileData in dirData.Files)
        {
            string relativePath = Path.Combine(dirData.RelativeDirectoryPath, fileData.FileName);
            fileIndex._pathIndex[relativePath] = fileData;

            string propertyIndexValue = fileData.PropertyIndexValue;
            if (!fileIndex._propertyIndex.ContainsKey(propertyIndexValue))
            {
                fileIndex._propertyIndex[propertyIndexValue] = [];
            }
            fileIndex._propertyIndex[propertyIndexValue].Add(fileData);
        }

        foreach (var subDir in dirData.SubDirs)
        {
            BuildFileIndexRecursive(subDir, fileIndex);
        }
    }
    #endregion
}
