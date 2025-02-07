using System.Collections.Generic;
using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Overrides.Actions.Data;

public class ItemOperation
{
    [JsonProperty] public IEnumerable<ItemQuantity> Items { get; set; } = [];
    [JsonProperty] public Dictionary<string, PropertyValue> Properties { get; set; } = [];
    [JsonProperty] public EntityId Owner { get; set; } = new();
    [JsonProperty] public bool BypassLock { get; set; }

    public class ItemQuantity
    {
        [JsonProperty] public ulong Id { get; set; }
        [JsonProperty] public string Name { get; set; } = "";
        [JsonProperty] public long Quantity { get; set; }
    }
}