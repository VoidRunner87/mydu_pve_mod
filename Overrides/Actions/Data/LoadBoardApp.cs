using System;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Overrides.Actions.Data;

public class LoadBoardApp
{
    [JsonProperty("factionId")] public ulong FactionId { get; set; }
    [JsonProperty("seed")] public int Seed { get; set; }
    [JsonProperty("territoryId")] public Guid TerritoryId { get; set; }
    [JsonProperty("constructId")] public ulong ConstructId { get; set; }
}