using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Common.Data;
using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class TemporaryMemoryCache<TKey, T>(TimeSpan expirationSpan) : IExpireCache
{
    // private object _lock = new();
    private readonly ConcurrentDictionary<TKey, CachedEntry<T>> _entries = new();

    public async Task<T> TryGetValue(TKey key, Func<Task<T>> defaultValueFn, Func<T, bool>? noCacheRule = null)
    {
        if (key == null)
        {
            return await defaultValueFn();
        }

        if (!_entries.TryGetValue(key, out var entry))
        {
            var data = await defaultValueFn();
            TrySetValue(key, data, noCacheRule);

            return data;
        }

        if (entry.IsExpired(DateTime.UtcNow))
        {
            var data = await defaultValueFn();
            TrySetValue(key, data, noCacheRule);
            
            return data;
        }

        return entry.Data;
    }

    public void TrySetValue(TKey key, T value, Func<T, bool>? noCacheRule = null)
    {
        if (key == null)
        {
            return;
        }
        
        if (noCacheRule != null && noCacheRule(value))
        {
            return;
        }

        if (!_entries.TryAdd(key, new CachedEntry<T>(value, DateTime.UtcNow + expirationSpan)))
        {
            _entries[key] = new CachedEntry<T>(value, DateTime.UtcNow + expirationSpan);
        }
    }

    public void Invalidate()
    {
        var now = DateTime.UtcNow;

        foreach (var kvp in _entries)
        {
            if (kvp.Value.IsExpired(now))
            {
                _entries.TryRemove(kvp.Key, out _);
            }
        }
    }
}