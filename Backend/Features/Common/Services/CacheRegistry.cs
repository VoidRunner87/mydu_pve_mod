using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace Mod.DynamicEncounters.Features.Common.Services;

public static class CacheRegistry
{
    public static Dictionary<string, MemoryCache> CacheMap { get; set; } = new();

    public static void Clear()
    {
        foreach (var kvp in CacheMap)
        {
            kvp.Value.Clear();
        }
    }
}