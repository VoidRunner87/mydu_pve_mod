using System;
using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Common.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ScriptContext(
    IServiceProvider serviceProvider,
    long? factionId,
    HashSet<ulong> playerIds, 
    Vec3 sector,
    Guid? territoryId
) : BaseContext
{
    public IServiceProvider ServiceProvider { get; set; } = serviceProvider;
    public long? FactionId { get; } = factionId;
    public HashSet<ulong> PlayerIds { get; set; } = playerIds;
    public Vec3 Sector { get; set; } = sector;
    public ulong? ConstructId { get; set; }
    public Guid? TerritoryId { get; set; } = territoryId;
    public event EventHandler<ScriptContextEventArgs>? OnEvent;

    public void RaiseEvent(ScriptContextEventArgs eventArgs)
    {
        OnEvent?.Invoke(this, eventArgs);
    }

    public ScriptContext WithConstructId(ulong constructId)
    {
        return new ScriptContext(ServiceProvider, FactionId, PlayerIds, Sector, TerritoryId)
        {
            ConstructId = constructId,
            Properties = Properties
        };
    }
}