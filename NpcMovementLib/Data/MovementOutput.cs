using System.Numerics;
using NpcMovementLib.Math;

namespace NpcMovementLib.Data;

public class MovementOutput
{
    public required Vec3 Position { get; init; }
    public required Vec3 Velocity { get; init; }
    public required Quaternion Rotation { get; init; }
}
