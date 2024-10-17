using System;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class TerritoryContainerItem
{
    public Guid Id { get; set; }
    public Guid TerritoryId { get; set; }
    public ulong ConstructId { get; set; }
    public ulong ElementId { get; set; }
}