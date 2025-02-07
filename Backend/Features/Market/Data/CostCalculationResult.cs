using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;

namespace Mod.DynamicEncounters.Features.Market.Data;

public readonly struct CostCalculationResult
{
    public required IEnumerable<Entry> Entries { get; init; }

    public readonly struct Entry
    {
        public required string InputItemName { get; init; }
        public required string OutputItemName { get; init; }
        public required IItemQuantity Quantity { get; init; }
        public required Quanta Price { get; init; }

        public Quanta GetUnitPrice() => Price / Quantity.GetReadableValue();

        public Entry CalculateForQuantity(IItemQuantity quantity)
        {
            return this with
            {
                Price = quantity.GetReadableValue() * GetUnitPrice(), 
                Quantity = quantity
            };
        }
    }

    public Quanta GetTotalCost()
    {
        return Entries.Sum(x => x.Price);
    }

    public CostCalculationResult CalculateForOutput(string outputItemName, IItemQuantity quantity)
    {
        var entries = new List<Entry>();
        
        foreach (var entry in Entries)
        {
            if (outputItemName == entry.OutputItemName)
            {
                entries.Add(entry.CalculateForQuantity(quantity));
                continue;
            }
            
            entries.Add(entry);
        }

        return new CostCalculationResult
        {
            Entries = entries
        };
    }
}