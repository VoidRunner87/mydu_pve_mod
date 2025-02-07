using Mod.DynamicEncounters.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Data;

public class ConstructTransformOutcome(bool constructExists, Vec3 position, Quat rotation) : IOutcome
{
    public bool ConstructExists { get; } = constructExists;
    public Vec3 Position { get; } = position;
    public Quat Rotation { get; } = rotation;

    public static ConstructTransformOutcome DoesNotExist() => new(false, new Vec3(), Quat.Identity);
}