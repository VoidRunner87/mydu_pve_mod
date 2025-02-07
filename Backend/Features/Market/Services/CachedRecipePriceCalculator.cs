using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Mod.DynamicEncounters.Features.Market.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;

namespace Mod.DynamicEncounters.Features.Market.Services;

public class CachedRecipePriceCalculator(IRecipePriceCalculator service) : IRecipePriceCalculator
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions
        { TrackStatistics = true, ExpirationScanFrequency = TimeSpan.FromSeconds(1) });
    
    public async Task<Dictionary<string, RecipeOutputData>> GetItemPriceMap()
    {
        if (_cache.TryGetValue("0", out var entry))
        {
            return (Dictionary<string, RecipeOutputData>)entry!;
        }

        var data = await service.GetItemPriceMap();

        _cache.Set("0", data, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(3)
        });

        return data;
    }
}