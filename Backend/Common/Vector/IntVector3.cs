using NQ;

namespace Mod.DynamicEncounters.Common.Vector;

public struct LongVector3(long x, long y, long z)
{
    public long X { get; set; } = x;
    public long Y { get; set; } = y;
    public long Z { get; set; } = z;

    public LongVector3(long v) : this(v, v, v)
    {
    }

    public LongVector3() : this(0, 0, 0)
    {
    }

    public LongVector3(Vec3 vec3) : this((long)vec3.x, (long)vec3.y, (long)vec3.z)
    {
    }

    public static LongVector3 operator -(LongVector3 a, LongVector3 b)
    {
        return new LongVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static LongVector3 operator *(LongVector3 a, LongVector3 b)
    {
        return new LongVector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }

    public static LongVector3 operator *(LongVector3 a, long scalar)
    {
        return new LongVector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }
}