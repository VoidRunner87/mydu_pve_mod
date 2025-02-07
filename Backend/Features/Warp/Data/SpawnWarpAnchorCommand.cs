using NQ;

namespace Mod.DynamicEncounters.Features.Warp.Data;

public class SpawnWarpAnchorCommand
{
    public PlayerId PlayerId { get; set; }
    public required Vec3 FromPosition { get; set; }
    public required Vec3 TargetPosition { get; set; }
    public required string ElementTypeName { get; set; } = "";
}