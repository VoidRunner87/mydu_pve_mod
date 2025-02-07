using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;

namespace Mod.DynamicEncounters.Features.Market.Data;

public readonly struct RecipeOutputData()
{
    public IItemQuantity Quantity { get; init; } = new Quantity(0);
    public Quanta Quanta { get; init; } = default;

    public double GetUnitPrice()
    {
        if (Quantity.Value == 0)
        {
            return 0;
        }
        
        return Quanta.Value / Quantity.GetReadableValue();
    }
}