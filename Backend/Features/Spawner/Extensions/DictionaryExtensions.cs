using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Spawner.Extensions;

public static class DictionaryExtensions
{
    public static void Merge<TKey, T>(this Dictionary<TKey, T> map, Dictionary<TKey, T> mapToMerge, bool @override = true)
    {
        foreach (var kvp in mapToMerge)
        {
            if (!map.TryAdd(kvp.Key, kvp.Value) && @override)
            {
                map[kvp.Key] = kvp.Value;
            }
        }
    }
}