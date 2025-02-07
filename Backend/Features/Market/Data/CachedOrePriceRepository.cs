using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;

namespace Mod.DynamicEncounters.Features.Market.Data;

public class CachedOrePriceRepository(IOrePriceRepository repository) : IOrePriceRepository
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    
    public async Task<Dictionary<string, Quanta>> GetOrePrices()
    {
        if (_cache.TryGetValue("0", out var orePriceCached) && orePriceCached is not null)
        {
            return (Dictionary<string, Quanta>)orePriceCached;
        }

        var orePriceNew = await repository.GetOrePrices();

        _cache.Set("0", orePriceNew, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.UtcNow + TimeSpan.FromHours(1)
        });

        return orePriceNew;
    }
}