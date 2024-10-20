using System.Collections.Generic;
using NQ;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public class ItemOperation
{
    public IEnumerable<ItemDefinition> Items { get; set; } = [];
    public Dictionary<string, PropertyValue> Properties { get; set; } = [];
    public EntityId Owner { get; set; } = new();
    public string OnSuccessCallbackUrl { get; set; } = "";
    public string OnFailCallbackUrl { get; set; } = "";

    public class ItemDefinition
    {
        public ulong Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public long Quantity { get; set; } = 0;
    }
}