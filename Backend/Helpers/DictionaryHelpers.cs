using System.Collections.Generic;

namespace Mod.DynamicEncounters.Helpers;

public static class DictionaryHelpers
{
    public static bool TryGetValue<T>(this Dictionary<string, object> dict, string key, out T outValue)
    {
        if (dict.TryGetValue(key, out var value))
        {
            outValue = (T)value;
            return true;
        }

        outValue = default;
        return false;
    }
}