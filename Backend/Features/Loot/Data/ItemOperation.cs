using System.Collections.Generic;
using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public class ItemOperation
{
    [JsonProperty] public IEnumerable<ItemDefinition> Items { get; set; } = [];
    [JsonProperty] public Dictionary<string, PropertyValue> Properties { get; set; } = [];
    [JsonProperty] public EntityId Owner { get; set; } = new();
    [JsonProperty] public string OnSuccessCallbackUrl { get; set; } = "";
    [JsonProperty] public string OnFailCallbackUrl { get; set; } = "";
    [JsonProperty] public bool BypassLock { get; set; }

    public class ItemDefinition
    {
        [JsonProperty] public ulong Id { get; set; }
        [JsonProperty] public string Name { get; set; } = "";
        [JsonProperty] public long Quantity { get; set; } = 0;
    }
}