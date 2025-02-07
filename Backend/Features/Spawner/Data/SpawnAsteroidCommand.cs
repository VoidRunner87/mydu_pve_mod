using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class SpawnAsteroidCommand
{
    [JsonProperty] public required int Tier { get; set; } = 3;
    [JsonProperty] public required Vec3 Position { get; set; }
    [JsonProperty] public required ulong Planet { get; set; } = 2;
    [JsonProperty] public required string Prefix { get; set; } = "A-";
    [JsonProperty] public required bool RegisterAsteroid { get; set; } = true;
    [JsonProperty] public required int Radius { get; set; } = 500;
    [JsonProperty] public required int AreaSize { get; set; } = 2048;
    [JsonProperty] public required int VoxelSize { get; set; } = 2048;
    [JsonProperty] public required int VoxelLod { get; set; } = 5;
    [JsonProperty] public required double Gravity { get; set; } = 1;
    [JsonProperty] public required JToken Data { get; set; } = string.Empty;
}