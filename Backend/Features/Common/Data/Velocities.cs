using NQ;

namespace Mod.DynamicEncounters.Features.Common.Data;

public struct Velocities(Vec3 linear, Vec3 angular)
{
    public Vec3 Linear { get; set; } = linear;
    public Vec3 Angular { get; set; } = angular;
}