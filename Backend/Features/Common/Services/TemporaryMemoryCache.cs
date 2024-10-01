using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class TemporaryMemoryCache<TKey, T>
{
    private readonly TimeSpan _expirationSpan;
    
    private readonly MemoryCache _cache = new(new MemoryCacheOptions
    {
        ExpirationScanFrequency = TimeSpan.FromSeconds(1/30d),
        TrackStatistics = true
    });

    public TemporaryMemoryCache(string id, TimeSpan expirationSpan)
    {
        _expirationSpan = expirationSpan;

        CacheRegistry.CacheMap.TryAdd(id, _cache);
    }
    
    public async Task<T> TryGetValue(TKey key, Func<Task<T>> defaultValueFn, Func<T, bool>? noCacheRule = null)
    {
        if (key == null)
        {
            return await defaultValueFn();
        }

        if (!_cache.TryGetValue(key, out var entry))
        {
            var data = await defaultValueFn();
            SetValue(key, data, noCacheRule);

            return data;
        }

        if (entry == null)
        {
            var data = await defaultValueFn();
            SetValue(key, data, noCacheRule);

            return data;
        }

        return (T)entry;
    }

    public void SetValue(TKey key, T value, Func<T, bool>? noCacheRule = null)
    {
        if (key == null)
        {
            return;
        }

        if (noCacheRule != null && noCacheRule(value))
        {
            return;
        }

        _cache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _expirationSpan
        });
    }
}