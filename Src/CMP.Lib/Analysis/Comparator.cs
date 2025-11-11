using CMP.Lib.Data;

namespace CMP.Lib.Analysis;

/// <summary>
/// Directory Tree and File Comparator
/// </summary>
/// <remarks>
/// This class provides functionality to compare two directory trees represented by DirData objects.
/// It analyzes the differences between files and directories in the source and target locations,
/// determining if files have been added, deleted, moved, modified, duplicated, or deduplicated.
/// 
/// It adds comparison results to FileData objects within the DirData structures,
/// allowing for detailed reporting of changes between the two directory trees.
/// </remarks>
public static class Comparator
{
    /// <summary>
    /// Compare two DirData objects representing source and target directory trees
    /// </summary>
    /// <param name="source">The source DirData</param>
    /// <param name="target">The target DirData</param>
    public static void CompareDirData(DirData source, DirData target)
    {
        // Prepare indexes for quick lookup in source and target
        FileIndex sourceIndex = FileIndex.FromDirData(source);
        FileIndex targetIndex = FileIndex.FromDirData(target);

        // First, find the not modified files and mark them as Equal
        // based on file paths, sizes, and CRCs.
        foreach (FileData sourceFile in sourceIndex.Files)
        {
            string relativePath = Path.Combine(sourceFile.RelativeDirectoryPath, sourceFile.FileName);
            if (targetIndex.TryGetFileData(relativePath, out var targetFile))
            {
                if (targetFile != null && sourceFile.Size == targetFile.Size && sourceFile.CRC == targetFile.CRC)
                {
                    sourceFile.CmpResult = new FileCmpResult
                    {
                        Result = CmpResult.Equal,
                        Files = [targetFile]
                    };
                    targetFile.CmpResult = new FileCmpResult
                    {
                        Result = CmpResult.Equal,
                        Files = [sourceFile]
                    };
                }
            }
        }

        // Further comparison logic to identify moved files
        foreach (string propertyIndexValue in sourceIndex.PropertyIndexes)
        {
            if (sourceIndex.TryGetFileDataByPropertyIndex(propertyIndexValue, out List<FileData> sourceFiles))
            {
                if (targetIndex.TryGetFileDataByPropertyIndex(propertyIndexValue, out List<FileData> targetFiles))
                {
                    // filter out already equal files
                    var notEqualSourceFiles = sourceFiles.Where(sf => sf.CmpResult == null || sf.CmpResult.Result != CmpResult.Equal).ToList();
                    var notEqualTargetFiles = targetFiles.Where(tf => tf.CmpResult == null || tf.CmpResult.Result != CmpResult.Equal).ToList();

                    int sourceCount = notEqualSourceFiles.Count;
                    int targetCount = notEqualTargetFiles.Count;

                    // should be deduplicated
                    if (notEqualSourceFiles.Any(sf => sf.FileName.Contains("Unlocked")) ||
                       notEqualTargetFiles.Any(tf => tf.FileName.Contains("Unlocked")))
                    {
                        Console.WriteLine("Debug");
                    }

                    if (sourceCount > 0 && targetCount > 0)
                    {
                        for (int i = 0; i < Math.Min(sourceCount, targetCount); i++)
                        {
                            FileData sourceFile = notEqualSourceFiles[i];
                            FileData targetFile = notEqualTargetFiles[i];

                            sourceFile.CmpResult = new FileCmpResult
                            {
                                Result = CmpResult.Moved,
                                Files = [targetFile]
                            };
                            targetFile.CmpResult = new FileCmpResult
                            {
                                Result = CmpResult.Moved,
                                Files = [sourceFile]
                            };
                        }
                    }
                }
            }
        }

        // Implementation of comparison logic goes here
        // This would involve comparing files and subdirectories,
        // and updating FileData objects with comparison results.
    }
}
