using System;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Newtonsoft.Json.Linq;
using NQ;

namespace Mod.DynamicEncounters.Features.Events.Data;

public class WreckDiscoveredEvent(ulong playerId, Vec3 sector, ulong? constructId, int groupSize) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; } = "wreck_discovered";
    public object Data { get; } = new { Sector = sector, ConstructId = constructId, GroupSize = groupSize };
    public int Value { get; } = 1;
    public ulong? PlayerId { get; } = playerId;

    public T GetData<T>()
    {
        var jObj = JObject.FromObject(Data);

        return jObj.ToObject<T>();
    }
}