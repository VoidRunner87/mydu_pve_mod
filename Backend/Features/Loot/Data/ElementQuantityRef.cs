using NQ;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public readonly struct ElementQuantityRef(ElementId? elementId, ElementTypeName elementTypeName, long quantity)
{
    public ElementId? ElementId { get; } = elementId;
    public ElementTypeName ElementTypeName { get; } = elementTypeName;
    public long Quantity { get; } = quantity;
}