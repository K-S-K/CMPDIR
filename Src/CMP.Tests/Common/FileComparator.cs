using System.Text;
using System.Security.Cryptography;

namespace CMP.Tests.Common;

public static class FileComparator
{
    private static string GetFileHash(string filename)
    {
        var clearBytes = File.ReadAllBytes(filename);
        var hashedBytes = SHA1.HashData(clearBytes);
        return ConvertBytesToHex(hashedBytes);
    }

    private static string ConvertBytesToHex(byte[] bytes)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < bytes.Length; i++)
        {
            sb.Append(bytes[i].ToString("x"));
        }
        return sb.ToString();
    }

    public static void CompareFiles(string fnActual, string fnExpctd, string errMsg)
    {
        string HashStockActual = GetFileHash(fnActual);
        string HashStockExpctd = GetFileHash(fnExpctd);
        if (HashStockActual != HashStockExpctd)
            throw new Exception(Environment.NewLine + errMsg);
    }
}
