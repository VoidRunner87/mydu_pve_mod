using System;

namespace Mod.DynamicEncounters.Features.Faction.Data;

public readonly struct TerritoryId(Guid id)
{
    public Guid Id { get; } = id;
    
    public static implicit operator TerritoryId(Guid id) => new(id);

    public static implicit operator Guid(TerritoryId territoryId) => territoryId.Id;
}