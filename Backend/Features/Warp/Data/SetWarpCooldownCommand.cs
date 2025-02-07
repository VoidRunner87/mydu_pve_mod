namespace Mod.DynamicEncounters.Features.Warp.Data;

public class SetWarpCooldownCommand
{
    public required ulong ConstructId { get; set; }
    public required string ElementTypeName { get; set; }
}