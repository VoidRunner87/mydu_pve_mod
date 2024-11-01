using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.Common.Data;

public abstract class BaseContext
{
    public ConcurrentDictionary<string, object> Properties { get; init; } = new();

    public bool TryGetProperty<T>(string name, out T value, T defaultValue)
    {
        if (Properties.TryGetValue(name, out var objVal))
        {
            if (objVal.GetType() is not T)
            {
                try
                {
                    value = JToken.FromObject(objVal).ToObject<T>();
                }
                catch (Exception e)
                {
                    var logger = ModBase.ServiceProvider.CreateLogger<BaseContext>();
                    logger.LogError(e, "Failed to parse Context Property {Prop}, Value {Value}", name, objVal);

                    value = defaultValue;
                    return false;
                }
            }
            else
            {
                value = (T)objVal;
            }

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
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (map == null)
        {
            return;
        }

        foreach (var kvp in map)
        {
            Properties.TryAdd(kvp.Key, kvp.Value);
        }
    }
}