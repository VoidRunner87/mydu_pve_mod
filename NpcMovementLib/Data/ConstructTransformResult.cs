using System.Numerics;
using NpcMovementLib.Math;

namespace NpcMovementLib.Data;

public class ConstructTransformResult
{
    public bool ConstructExists { get; set; }
    public Vec3 Position { get; set; }
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
}
