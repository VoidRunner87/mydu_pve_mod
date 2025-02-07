using System;

namespace Mod.DynamicEncounters.Features.Common.Extensions;

public static class ByteArrayExtensions
{
    public static byte[] SkipBytes(this byte[] data, int bytesToSkip)
    {
        if (data.Length <= bytesToSkip)
            return [];

        var result = new byte[data.Length - bytesToSkip];
        Buffer.BlockCopy(data, bytesToSkip, result, 0, result.Length);

        return result;
    }
}