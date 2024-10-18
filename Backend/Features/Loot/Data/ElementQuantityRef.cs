using System.Collections.Generic;
using NQ;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public readonly struct ElementQuantityRef(ElementTypeName elementTypeName, long quantity)
{
    public ElementTypeName ElementTypeName { get; } = elementTypeName;
    public long Quantity { get; } = quantity;
    public Dictionary<string, PropertyValue> Properties { get; private init; } = [];

    public ElementQuantityRef WithProperty(string propName, PropertyValue value)
    {
        return new ElementQuantityRef(ElementTypeName, Quantity)
        {
            Properties = new Dictionary<string, PropertyValue>(Properties)
            {
                { propName, value }
            }
        };
    }
}