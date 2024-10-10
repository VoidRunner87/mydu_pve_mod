using System.Collections.Concurrent;

namespace Mod.DynamicEncounters.Features.Spawner.Extensions;

public static class ConcurrentDictionaryExtensions
{
    public static void Set<TKey, T>(this ConcurrentDictionary<TKey, T> dictionary, TKey key, T value)
    {
        dictionary.AddOrUpdate(
            key,
            _ => value,
            (_, _) => value
        );
    }

    public static T GetOrDefault<TKey, T>(this ConcurrentDictionary<TKey, T> dictionary, TKey key, T defaultValue = default)
    {
        if (key == null)
        {
            return defaultValue;
        }
        
        if (!dictionary.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        return value;
    }
}