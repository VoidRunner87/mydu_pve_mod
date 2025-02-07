using System;
using Microsoft.Extensions.Caching.Memory;
using Mod.DynamicEncounters.Overrides.Common.Data;
using Mod.DynamicEncounters.Overrides.Common.Interfaces;

namespace Mod.DynamicEncounters.Overrides.Common.Services;

public class CachedConstructDataService : ICachedConstructDataService
{
    private readonly MemoryCache _cache = new(
        new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromSeconds(1/20d)
        }
    );
    
    public ConstructData? Get(ulong constructId)
    {
        if (_cache.TryGetValue(constructId, out var data))
        {
            return (ConstructData?)data;
        }

        return default;
    }

    public void Set(ulong constructId, ConstructData data)
    {
        _cache.Set(constructId, data, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(60)
        });
    }
}