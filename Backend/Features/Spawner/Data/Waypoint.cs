using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class Waypoint(Vec3 position)
{
    public Vec3 Position { get; } = position;
    public bool Visited { get; set; }
}