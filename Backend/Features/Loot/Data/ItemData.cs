using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public class ItemData
{
    public IEnumerable<ItemAndQuantity> Entries { get; set; } = [];
    
    public readonly struct ItemAndQuantity(string itemName, long quantity)
    {
        public string ItemName { get; } = itemName;
        public long Quantity { get; } = quantity;
    }
}