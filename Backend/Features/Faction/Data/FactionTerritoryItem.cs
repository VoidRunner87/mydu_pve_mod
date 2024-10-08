using System;

namespace Mod.DynamicEncounters.Features.Faction.Data;

public class FactionTerritoryItem
{
    public Guid Id { get; set; }
    public long FactionId { get; set; }
    public Guid TerritoryId { get; set; }
    public bool IsPermanent { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int SectorCount { get; set; }
}