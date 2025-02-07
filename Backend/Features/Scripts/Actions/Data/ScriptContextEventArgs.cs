using System;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ScriptContextEventArgs(string eventType) : EventArgs
{
    public string EventType { get; set; } = eventType;
    public virtual JToken? Data { get; set; }

    public T? GetData<T>()
    {
        if (Data == null) return default;
        
        return Data.ToObject<T>();
    }
}