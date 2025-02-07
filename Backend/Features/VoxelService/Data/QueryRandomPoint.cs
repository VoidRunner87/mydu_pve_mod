using NQ;

namespace Mod.DynamicEncounters.Features.VoxelService.Data;

public class QueryRandomPoint
{
    public required ConstructId ConstructId { get; init; }
    public required Vec3 FromLocalPosition { get; init; }
}