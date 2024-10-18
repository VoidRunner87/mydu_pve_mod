namespace Mod.DynamicEncounters.Features.Loot.Data;

public readonly struct ElementQuantityRef(ElementTypeName elementTypeName, long quantity)
{
    public ElementTypeName ElementTypeName { get; } = elementTypeName;
    public long Quantity { get; } = quantity;
}