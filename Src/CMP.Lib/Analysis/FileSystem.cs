using CMP.Lib.Analysis.FailureControl;
using System.Collections.Concurrent;

namespace CMP.Lib.Analysis;

/// <summary>
/// Represents a file system for analysis purposes.
/// </summary>
public class FileSystem
{
    #region -> Internal Fields
    private readonly bool ErrorHandlingTesting = false;
    private readonly ConcurrentStack<AnalysisException> _errors = [];

    public IEnumerable<AnalysisException> Errors => _errors;

    private long _failedFileCount = 0;
    private long _processedFileCount = 0;
    #endregion


    #region -> Properties
    public long FailedFileCount => _failedFileCount;
    public long ProcessedFileCount => _processedFileCount;
    #endregion


    #region -> Methods
    public bool TryGetDirectoryEntries(string dirPath, out string[] result)
    {
        try
        {
            result = GetDirectoryEntries(dirPath);
            return true;
        }
        catch (AnalysisException ex)
        {
            Interlocked.Increment(ref _failedFileCount);
            _errors.Push(ex);
            result = [];
            return false;
        }
    }

    public bool TryGetFileNames(string dirPath, out string[] result)
    {
        try
        {
            GetFileNames(dirPath, out result);
            return true;
        }
        catch (AnalysisException ex)
        {
            Interlocked.Increment(ref _failedFileCount);
            _errors.Push(ex);
            result = [];
            return false;
        }
    }

    public bool TryGetFileInfo(string filePath, out FileInfo? fileInfo)
    {
        try
        {
            try
            {
                GetFileInfo(filePath, out fileInfo);
                long size = fileInfo.Length;
                return fileInfo != null;
            }
            catch (Exception e)
            {
                Console.Beep();
                throw new FileMetadataReadException(e, filePath);
            }
        }
        catch (AnalysisException ex)
        {
            Interlocked.Increment(ref _failedFileCount);
            _errors.Push(ex);
            fileInfo = null;
            return false;
        }
    }

    public bool TryComputeCrc32(string filePath, out uint crc)
    {
        crc = 0;
        bool result = false;
        try
        {
            ComputeCrc32(filePath, out crc);
            result = true;
        }
        catch (AnalysisException ex)
        {
            Interlocked.Increment(ref _failedFileCount);
            _errors.Push(ex);
        }
        finally
        {
            // Calculate processed file count for progress
            Interlocked.Increment(ref _processedFileCount);
        }

        return result;
    }
    #endregion


    #region -> Implementation
    private string[] GetDirectoryEntries(string dirPath)
    {
        string[] result;
        try
        {
            if (ErrorHandlingTesting && dirPath.Contains("Entrance"))
            {
                throw new Exception("Test exception at 'Entrance'");
            }

            result = Directory.GetDirectories(dirPath);
        }
        catch (Exception ex)
        {
            throw new DirectoryListingException(ex, dirPath);
        }

        return result;
    }

    private bool GetFileNames(string dirPath, out string[] result)
    {
        try
        {
            if (ErrorHandlingTesting && dirPath.Contains("Weather Station"))
            {
                throw new Exception("Test exception at 'Weather Station'");
            }

            result = Directory.GetFiles(dirPath);
        }
        catch (Exception ex)
        {
            throw new FileListingException(ex, dirPath);
        }

        return true;
    }

    private bool GetFileInfo(string filePath, out FileInfo fileInfo)
    {
        try
        {
            if (ErrorHandlingTesting && filePath.Contains("DSCF1898.jpg"))
            {
                throw new Exception("Test exception at 'DSCF1898.jpg'");
            }

            fileInfo = new(filePath);
        }
        catch (Exception ex)
        {
            throw new FileMetadataReadException(ex, filePath);
        }

        return true;
    }

    private bool ComputeCrc32(string filePath, out uint crc)
    {
        crc = 0;
        bool result = false;
        try
        {
            if (ErrorHandlingTesting && ProcessedFileCount == 35)
            {
                throw new Exception("Test exception at file 35");
            }

            crc = Crc32.ComputeFile(filePath);
            result = true;
        }
        catch (Exception ex)
        {
            throw new FileContentReadException(ex, filePath);
        }

        return result;
    }
    #endregion
}
