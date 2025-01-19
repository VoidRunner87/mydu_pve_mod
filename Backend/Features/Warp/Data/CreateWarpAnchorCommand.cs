using NQ;

namespace Mod.DynamicEncounters.Features.Warp.Data;

public class CreateWarpAnchorCommand
{
    public required PlayerId PlayerId { get; set; }
    public required Vec3? TargetPosition { get; set; }
    public double Offset { get; set; } = 12000D;
}