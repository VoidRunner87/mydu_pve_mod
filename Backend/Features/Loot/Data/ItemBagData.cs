using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Loot.Interfaces;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public class ItemBagData(double maxBudget)
{
    public double MaxBudget { get; set; } = maxBudget;
    public double CurrentCost { get; set; } = 0;
    private IList<ItemAndQuantity> _entries { get; init; } = [];
    public IList<ElementReplace> ElementsToReplace { get; set; } = [];
    
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
        
        _entries.Add(itemAndQuantity);
        CurrentCost += cost;

        return true;
    }

    public IEnumerable<ItemAndQuantity> GetEntries() => _entries;
}