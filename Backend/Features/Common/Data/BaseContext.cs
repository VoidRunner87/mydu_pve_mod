using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Common.Data;

public abstract class BaseContext
{
    public ConcurrentDictionary<string, object> Properties { get; } = new();
    
    public bool TryGetProperty<T>(string name, out T value, T defaultValue)
    {
        if (Properties.TryGetValue(name, out var objVal))
        {
            value = (T)objVal;
            return true;
        }

        value = defaultValue;
        return false;
    }

    public void SetProperty<T>(string name, T value)
    {
        Properties.AddOrUpdate(
            name,
            _ => value,
            (_, _) => value
        );
    }

    public void RemoveProperty(string name)
    {
        Properties.TryRemove(name, out _);
    }

    public void AddProperties(Dictionary<string, object> map)
    {
        foreach (var kvp in map)
        {
            Properties.TryAdd(kvp.Key, kvp.Value);
        }
    }
}