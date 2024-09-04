using NQ;

namespace Mod.DynamicEncounters.Helpers;

public static class QuatHelpers
{
    public static Quat Multiply(Quat q1, Quat q2)
    {
        return new Quat{
            x = q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y,
            y = q1.w * q2.y - q1.x * q2.z + q1.y * q2.w + q1.z * q2.x,
            z = q1.w * q2.z + q1.x * q2.y - q1.y * q2.x + q1.z * q2.w,
            w = q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z
        };
    }
    
    // Inverse of the quaternion
    public static Quat Inverse(Quat q)
    {
        return new Quat { x = -q.x, y = -q.y, z = -q.z, w = q.w };
    }
}