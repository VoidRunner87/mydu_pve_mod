using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Loot.Interfaces;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public class ItemBagData
{
    public required string Name { get; set; } = string.Empty;
    public required double MaxBudget { get; set; }
    public double CurrentCost { get; set; } = 0;
    public IList<ItemAndQuantity> Entries { get; init; } = [];
    public IList<ElementReplace> ElementsToReplace { get; set; } = [];
    public required IEnumerable<string> Tags { get; set; }

    public readonly struct ElementReplace(string elementName, string replaceElementName, long quantity)
    {
        public string ElementName { get; } = elementName;
        public string ReplaceElementName { get; } = replaceElementName;
        public long Quantity { get; } = quantity;
    }

    public readonly struct ItemAndQuantity(string itemName, IQuantity quantity)
    {
        public string ItemName { get; } = itemName;
        public IQuantity Quantity { get; } = quantity;
    }

    public bool AddEntry(long cost, ItemAndQuantity itemAndQuantity)
    {
        if (cost + CurrentCost > MaxBudget)
        {
            return false;
        }

        Entries.Add(itemAndQuantity);
        CurrentCost += cost;

        return true;
    }

    public IEnumerable<ItemAndQuantity> GetEntries() => Entries;

    public ItemBagData WithName(string name)
    {
        Name = name;

        return this;
    }
}