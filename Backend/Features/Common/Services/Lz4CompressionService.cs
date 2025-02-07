using System;
using System.Text;
using K4os.Compression.LZ4;

namespace Mod.DynamicEncounters.Features.Common.Services;

public static class Lz4CompressionService
{
    public static byte[] Compress(string input)
    {
        var inputData = Encoding.UTF8.GetBytes(input);
        var compressed = new byte[LZ4Codec.MaximumOutputSize(inputData.Length)];
        var compressedLength = LZ4Codec.Encode(inputData, compressed);

        Array.Resize(ref compressed, compressedLength);

        return compressed;
    }

    public static byte[] Decompress(byte[] compressedData, int bufferSize)
    {
        var decompressedData = new byte[bufferSize];
        var decompressedLength = LZ4Codec.Decode(
            compressedData, 0, compressedData.Length,
            decompressedData, 0, decompressedData.Length
        );

        return decompressedData.AsSpan(0, decompressedLength).ToArray();
    }

    public static int ReadDecompressedSize(byte[] data, int startIndex = 0)
    {
        if (data == null || data.Length < startIndex + 4)
            throw new ArgumentException("Byte array is too small to read 4 bytes.");

        return data[startIndex]
               | (data[startIndex + 1] << 8)
               | (data[startIndex + 2] << 16)
               | (data[startIndex + 3] << 24);
    }
    
    public static byte[] PrependDecompressedSize(int size, byte[] data)
    {
        var result = new byte[data.Length + 4];

        result[0] = (byte)(size & 0xFF);
        result[1] = (byte)((size >> 8) & 0xFF);
        result[2] = (byte)((size >> 16) & 0xFF);
        result[3] = (byte)((size >> 24) & 0xFF);

        // Copy the original data after the 4-byte header
        Buffer.BlockCopy(data, 0, result, 4, data.Length);

        return result;
    }
}