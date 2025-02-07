using System.Numerics;
using NQ;

namespace Mod.DynamicEncounters.Overrides.Common;

public static class VectorMathHelper
{
    public static Vector3 CalculateWorldPosition(
        Vector3 relativePosition,
        Vector3 referencePosition,
        Quaternion referenceRotation
    )
    {
        // Step 1: Rotate the relative position into world space
        var rotatedPosition = Vector3.Transform(relativePosition, referenceRotation);

        // Step 2: Translate the rotated position by the reference's world position
        var worldPosition = rotatedPosition + referencePosition;

        return worldPosition;
    }
    
    public static Vector3 ToVector3(this Vec3 v)
    {
        return new Vector3((float)v.x, (float)v.y, (float)v.z);
    }
    
    public static Quaternion ToQuat(this Quat q)
    {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }
    
    public static Vec3 ToNqVec3(this Vector3 v)
    {
        return new Vec3 { x = v.X, y = v.Y, z = v.Z };
    }
}