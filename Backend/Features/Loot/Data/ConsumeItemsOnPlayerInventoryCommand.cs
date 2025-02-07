using System.Collections.Generic;
using NQ;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public class ConsumeItemsOnPlayerInventoryCommand(PlayerId playerId, IEnumerable<ElementQuantityRef> items)
{
    public PlayerId PlayerId { get; } = playerId;
    public IEnumerable<ElementQuantityRef> Items { get; } = items;
}