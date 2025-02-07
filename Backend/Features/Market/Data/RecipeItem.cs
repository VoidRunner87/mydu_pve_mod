using Mod.DynamicEncounters.Features.Market.Interfaces;

namespace Mod.DynamicEncounters.Features.Market.Data;

public class RecipeItem(
    string itemName,
    ulong itemId,
    IItemQuantity quantity
)
{
    public string ItemName { get; } = itemName;
    public ulong ItemId { get; } = itemId;
    public IItemQuantity Quantity { get; } = quantity;
}