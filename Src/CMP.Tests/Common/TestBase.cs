using System.Text;
using System.Xml.Linq;
using System.Reflection;

namespace CMP.Tests.Common;

/// <summary>
/// Base class for unit tests
/// </summary>
/// <remarks>Contains methods for saving and comparing test results</remarks>   
/// <remarks>Contains methods for comparing double values</remarks>
public class TestBase
{
    /// <summary>
    /// Lock object for directory creation
    /// </summary>
    private readonly object lock_dir = new();

    /// <summary>
    /// Results export path
    /// </summary>
    private string inner_dir = string.Empty;

    /// <summary>
    /// Results export path
    /// </summary>
    protected string ResultDir
    {
        get
        {
            lock (lock_dir)
            {
                if (string.IsNullOrEmpty(inner_dir))
                {
                    inner_dir = Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
                    if (!Directory.Exists(inner_dir)) Directory.CreateDirectory(inner_dir);
                }
                return inner_dir;
            }
        }
    }

    /// <summary>
    /// Creates and returns a subdirectory path within the test results directory
    /// </summary>
    /// <param name="subDirName">Name of the subdirectory</param>
    /// <returns>Full path to the subdirectory</returns>
    protected string TestSubDir(string subDirName)
    {
        string path = Path.Combine(ResultDir, subDirName);
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// Reads string value from environment variable
    /// </summary>
    /// <param name="envVariableName">Environment Variable Name</param>
    /// <returns>String Value</returns>
    public static string EnVar(string envVariableName)
    {
        string? strVal = Environment.GetEnvironmentVariable(envVariableName) ??
            throw new EnvironmentVariableNotFoundException(envVariableName);

        return strVal;
    }
    
    /// <summary>
    /// Results export
    /// </summary>
    /// <param name="xeActual">Actual data</param>
    /// <param name="xeExpctd">Expected data</param>
    /// <param name="subDirPath">Directory name</param>
    /// <param name="fnPrefix">Filename prefix</param>
    /// <remarks>Actual and expected data are saved in XML format</remarks>
    /// <remarks>Filename prefix is used to create filenames</remarks>
    /// <remarks>Directory name is used to create subdirectories</remarks>
    /// <remarks>Actual and expected data are saved in separate files</remarks>
    protected void ResultSave(XElement xeActual, XElement xeExpctd, string subDirPath, string fnPrefix)
    {
        string path = Path.Combine(ResultDir, subDirPath);
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        string fileNameExpctd = Path.Combine(path, string.Format("{0}.Expctd.xml", fnPrefix));
        string fileNameActual = Path.Combine(path, string.Format("{0}.Actual.xml", fnPrefix));

        new XDocument(xeActual).Save(fileNameActual);
        new XDocument(xeExpctd).Save(fileNameExpctd);
    }

    /// <summary>
    /// Results export
    /// </summary>
    /// <param name="txtActual">Actual data</param>
    /// <param name="xeExpctd">Expected data</param>
    /// <param name="subDirPath">Filename prefix</param>
    /// <param name="fnPrefix">Filename prefix</param>
    protected void ResultSave(string txtActual, string txtExpctd, string subDirPath, string fnPrefix)
    {
        string fileNameExpctd = Path.Combine(ResultDir, subDirPath, string.Format("{0}.Expctd.txt", fnPrefix));
        string fileNameActual = Path.Combine(ResultDir, subDirPath, string.Format("{0}.Actual.txt", fnPrefix));
        string? dir = Path.GetDirectoryName(fileNameActual);

        ArgumentNullException.ThrowIfNullOrWhiteSpace(dir, nameof(dir));

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (File.Exists(fileNameActual)) File.Delete(fileNameActual);
        File.WriteAllText(fileNameActual, txtActual, Encoding.UTF8);

        if (File.Exists(fileNameExpctd)) File.Delete(fileNameExpctd);
        File.WriteAllText(fileNameExpctd, txtExpctd, Encoding.UTF8);
    }

    /// <summary>
    /// Compares test result with expectations
    /// </summary>
    /// <param name="xeActual">Actual data</param>
    /// <param name="xeExpctd">Expected data</param>
    /// <param name="subDirPath">Filename prefix</param>
    /// <param name="fnPrefix">Filename prefix</param>
    /// <param name="errMsg">Error message</param>
    protected void CompareResult(XElement xeActual, XElement xeExpctd, string subDirPath, string fnPrefix, string errMsg)
    {
        string strActual = xeActual.ToString();
        string strExpctd = xeExpctd.ToString();
        ResultSave(xeActual, xeExpctd, subDirPath, fnPrefix);
        if (strExpctd != strActual)
        {
            throw new Exception(errMsg);
        }
    }

    /// <summary>
    /// Compares test result with expectations
    /// </summary>
    /// <param name="txtActual">Actual data</param>
    /// <param name="txtExpctd">Expected data</param>
    /// <param name="subDirPath">Filename prefix</param>
    /// <param name="fnPrefix">Filename prefix</param>
    protected bool CompareResult(string txtActual, string txtExpctd, string subDirPath, string fnPrefix, out string path)
    {
        path = Path.Combine(ResultDir, subDirPath);

        string txtActualNormalized = NormalizeNewlines(txtActual);
        string txtExpctdNormalized = NormalizeNewlines(txtExpctd);

        ResultSave(txtActualNormalized, txtExpctdNormalized, subDirPath, fnPrefix);
        return txtActualNormalized == txtExpctdNormalized;
    }

    protected void CompareFiles(string fnActual, string fnExpctd, string errMsg)
    {
        FileComparator.CompareFiles(fnActual, fnExpctd, errMsg);
    }

    /// <summary>
    /// Compare two double values
    /// </summary>
    /// <param name="a">First value</param>
    /// <param name="b">Second value</param>
    /// <param name="tolerance">Tolerance</param>
    /// <returns>True if values are equal</returns>
    /// <remarks>Default tolerance is 1e-10</remarks>
    /// <remarks>Values are considered equal if their difference is less than tolerance</remarks>
    protected static bool AreEqual(double a, double b, double tolerance = 1e-10)
    {
        return Math.Abs(a) - Math.Abs(b) < tolerance;
    }

    /// <summary>
    /// Get the content of a resource file as a string
    /// </summary>
    /// <param name="resourceName">Resource name</param>
    /// <returns>Resource content</returns>
    /// <remarks>Resource name is the full name of the resource file</remarks>
    protected string GetResourceFileContent(string resourceName)
    {
        Assembly asm = this.GetType().Assembly;

        using Stream? stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new Exception($"The resource {resourceName} does not exists");

        using StreamReader reader = new(stream);
        string resourceContent = reader.ReadToEnd();
        return resourceContent;
    }

    /// <summary>
    /// Get the content of a resource file as an array of strings
    /// </summary>
    /// <param name="resourceName">Resource name</param>
    /// <returns>Resource content</returns>
    /// <remarks>Resource name is the full name of the resource file</remarks>
    protected string[] GetResourceFileLines(string resourceName)
    {
        string content = GetResourceFileContent(resourceName);
        return content.Split(
            ["\r\n", "\r", "\n", Environment.NewLine],
            StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Normalize newlines in a string to '\n'
    /// </summary>
    /// <param name="s">Input string</param>
    /// <returns>String with normalized newlines</returns>
    private static string NormalizeNewlines(string s) =>
        s.Replace("\r\n", "\n").Replace("\r", "\n");
}
