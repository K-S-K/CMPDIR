using System;
using System.IO;

namespace CMP.Lib.Analysis;

public static class Crc32
{
    private static readonly uint[] Table = CreateTable();

    // Standard polynomial used in Ethernet, ZIP, etc.
    private const uint Polynomial = 0xEDB88320u;

    private static uint[] CreateTable()
    {
        uint[] table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
                crc = (crc & 1) != 0 ? (Polynomial ^ (crc >> 1)) : (crc >> 1);
            table[i] = crc;
        }
        return table;
    }

    public static uint ComputeFileByByte(string fileName)
    {
        uint crc = 0xFFFFFFFFu;

        using (var stream = File.OpenRead(fileName))
        {
            int b;
            while ((b = stream.ReadByte()) != -1)
            {
                crc = Table[(crc ^ (byte)b) & 0xFF] ^ (crc >> 8);
            }
        }

        return ~crc; // Final XOR
    }

    public static uint ComputeFile(string fileName)
    {
        uint crc = 0xFFFFFFFFu;

        const int BufferSize = 1024 * 1024 * 8; // 8 MB
        byte[] buffer = new byte[BufferSize];

        using var stream = new FileStream(
            fileName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.SequentialScan);

        int bytesRead;
        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                crc = Table[(crc ^ buffer[i]) & 0xFF] ^ (crc >> 8);
            }
        }

        return ~crc;
    }

    public static string ComputeFileHex(string fileName) =>
        ComputeFile(fileName).ToString("X8"); // Uppercase 8-digit hex
}
