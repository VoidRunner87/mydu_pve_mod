using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;

public class SkillItem
{
    [JsonProperty] public bool Active { get; set; } = true;
    [JsonProperty] public required string Name { get; set; } = "null";
    [JsonProperty] public required double CooldownSeconds { get; set; } = 60D;
    [JsonProperty] public string? ItemTypeName { get; set; }
}