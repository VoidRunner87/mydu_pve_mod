using NQ;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public class SpawnItemOnRandomContainersAroundAreaCommand
{
    public required ulong InstigatorConstructId { get; init; }
    public required ItemBagData ItemBag { get; init; }
    public required Vec3 Position { get; init; }
    public required double Radius { get; init; }
    public int Limit { get; init; } = 5;
}