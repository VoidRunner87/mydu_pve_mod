using System.Collections.Generic;
using NQ;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public class SpawnItemsOnPlayerInventoryCommand(
    PlayerId playerId,
    IEnumerable<ElementQuantityRef> items,
    Dictionary<string, PropertyValue> properties
)
{
    public PlayerId PlayerId { get; } = playerId;
    public IEnumerable<ElementQuantityRef> Items { get; } = items;
    public Dictionary<string, PropertyValue> Properties { get; set; } = properties;
}