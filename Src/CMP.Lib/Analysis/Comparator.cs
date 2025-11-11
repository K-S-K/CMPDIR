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
                if (targetFile != null)
                {
                    CmpResult cmpResult =
                        sourceFile.Size == targetFile.Size && sourceFile.CRC == targetFile.CRC
                        ? CmpResult.Equal
                        : CmpResult.Modified;

                    sourceFile.CmpResult = new FileCmpResult
                    {
                        Result = cmpResult,
                        Files = [targetFile]
                    };
                    targetFile.CmpResult = new FileCmpResult
                    {
                        Result = cmpResult,
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

        // Further comparison logic to identify added, deleted, duplicated, and deduplicated files

        // Common Property Index Values collection
        HashSet<string> commonPropertyIndex = [.. sourceIndex.PropertyIndexes];
        commonPropertyIndex.UnionWith(targetIndex.PropertyIndexes);


        foreach (string propertyIndexValue in commonPropertyIndex)
        {
            sourceIndex.TryGetFileDataByPropertyIndex(propertyIndexValue, out List<FileData> sourceFiles);
            targetIndex.TryGetFileDataByPropertyIndex(propertyIndexValue, out List<FileData> targetFiles);

            // filter out "Equal" and "Moved" or "Modified" files
            List<FileData> notAssignedSourceFiles = sourceFiles.Where(sf => sf.CmpResult == null).ToList();
            List<FileData> notAssignedTargetFiles = targetFiles.Where(tf => tf.CmpResult == null).ToList();

            int fullSourceCount = sourceFiles.Count;
            int fullTargetCount = targetFiles.Count;

            int notAssignedSourceFilesCount = notAssignedSourceFiles.Count;
            int notAssignedTargetFilesCount = notAssignedTargetFiles.Count;

            if (fullSourceCount > 0 && fullTargetCount > 0)
            {
                if (notAssignedSourceFilesCount > 0)
                {
                    for (int i = 0; i < notAssignedSourceFilesCount; i++)
                    {
                        FileData sourceFile = notAssignedSourceFiles[i];
                        sourceFile.CmpResult = new FileCmpResult
                        {
                            Result = CmpResult.Deduplicated,
                            Files = targetFiles
                        };
                    }
                }
                else if (notAssignedTargetFilesCount > 0)
                {
                    for (int i = 0; i < notAssignedTargetFilesCount; i++)
                    {
                        FileData targetFile = notAssignedTargetFiles[i];
                        targetFile.CmpResult = new FileCmpResult
                        {
                            Result = CmpResult.Duplicated,
                            Files = sourceFiles
                        };
                    }
                }
            }
            else if (fullSourceCount > 0 && fullTargetCount == 0)
            {
                foreach (var sourceFile in notAssignedSourceFiles)
                {
                    sourceFile.CmpResult = new FileCmpResult
                    {
                        Result = CmpResult.Deleted,
                        Files = []
                    };
                }
            }
            else if (fullTargetCount > 0 && fullSourceCount == 0)
            {
                foreach (var targetFile in notAssignedTargetFiles)
                {
                    targetFile.CmpResult = new FileCmpResult
                    {
                        Result = CmpResult.Added,
                        Files = []
                    };
                }
            }
        }
    }
}
