using Mod.DynamicEncounters.Features.Loot.Interfaces;
using NQutils.Def;

namespace Mod.DynamicEncounters.Features.Loot.Extensions;

public static class BaseObjectExtensions
{
    public static IQuantity GetQuantityForElement(this BaseItem baseItem, long quantity)
    {
        return baseItem.InventoryType == "material" ? new LitreQuantity(quantity) : new DefaultQuantity(quantity);
    }
}