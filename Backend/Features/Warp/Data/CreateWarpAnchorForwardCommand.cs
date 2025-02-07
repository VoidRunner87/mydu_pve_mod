using NQ;

namespace Mod.DynamicEncounters.Features.Warp.Data;

public class CreateWarpAnchorForwardCommand
{
    public required PlayerId PlayerId { get; set; }
    public required double Distance { get; set; }
}