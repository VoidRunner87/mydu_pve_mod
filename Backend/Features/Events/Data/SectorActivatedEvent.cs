using System;
using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Newtonsoft.Json.Linq;
using NQ;

namespace Mod.DynamicEncounters.Features.Events.Data;

public class SectorActivatedEvent(IEnumerable<ulong> playerIds, ulong? playerId, Vec3 sector, ulong? constructId, int groupSize) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; } = "sector_activated";
    public object Data { get; } = new { PlayerIds = playerIds, Sector = sector, ConstructId = constructId, GroupSize = groupSize };
    public int Value { get; } = 1;
    public ulong? PlayerId { get; } = playerId;

    public T GetData<T>()
    {
        var jObj = JObject.FromObject(Data);

        return jObj.ToObject<T>();
    }
}