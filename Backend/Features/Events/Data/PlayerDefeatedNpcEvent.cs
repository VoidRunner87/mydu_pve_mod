using System;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Newtonsoft.Json.Linq;
using NQ;

namespace Mod.DynamicEncounters.Features.Events.Data;

public class PlayerDefeatedNpcEvent(ulong playerId, Vec3 sector, ulong? constructId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; } = "player_defeated_npc";
    public object Data { get; } = new { Sector = sector, ConstructId = constructId };
    public int Value { get; } = 1;
    public ulong? PlayerId { get; } = playerId;
    public T GetData<T>()
    {
        var jObj = JObject.FromObject(Data);

        return jObj.ToObject<T>();
    }
}