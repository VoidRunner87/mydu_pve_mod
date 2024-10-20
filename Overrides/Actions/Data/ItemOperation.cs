using System.Collections.Generic;
using NQ;

namespace Mod.DynamicEncounters.Overrides.Actions.Data;

public class ItemOperation
{
    public IEnumerable<ItemQuantity> Items { get; set; } = [];
    public Dictionary<string, PropertyValue> Properties { get; set; } = [];
    public EntityId Owner { get; set; } = new();

    public class ItemQuantity
    {
        public ulong Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public long Quantity { get; set; } = 0;
    }
}