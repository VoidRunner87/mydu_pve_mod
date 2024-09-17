using System.Numerics;
using NQ;

namespace Mod.DynamicEncounters.Helpers;

public static class QuaternionConversionHelpers
{
    public static Quat ToNqQuat(this Quaternion q)
    {
        return new Quat { w = q.W, z = q.Z, y = q.Y, x = q.X };
    }

    public static Quaternion ToQuat(this Quat q)
    {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }
    
    public static MathNet.Spatial.Euclidean.Quaternion ToMnQuat(this Quat q)
    {
        return new MathNet.Spatial.Euclidean.Quaternion(q.x, q.y, q.z, q.w);
    }
}