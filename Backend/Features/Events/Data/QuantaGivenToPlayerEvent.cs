using System;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Newtonsoft.Json.Linq;
using NQ;

namespace Mod.DynamicEncounters.Features.Events.Data;

public class QuantaGivenToPlayerEvent(ulong playerId, Vec3 sector, ulong? constructId, int groupSize, ulong amount) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; } = "quanta_given_to_player";
    public object Data { get; } = new { Amount = amount, Sector = sector, ConstructId = constructId, GroupSize = groupSize };
    public int Value { get; } = 1;
    public ulong? PlayerId { get; } = playerId;

    public T GetData<T>()
    {
        var jObj = JObject.FromObject(Data);

        return jObj.ToObject<T>();
    }
}