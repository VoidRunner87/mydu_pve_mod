using System;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Overrides.Actions.Data;

public class AbandonQuest
{
    [JsonProperty("questId")]
    public Guid QuestId { get; set; }
    [JsonProperty("playerId")]
    public ulong PlayerId { get; set; }
}