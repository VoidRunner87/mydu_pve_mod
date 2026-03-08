using System.Numerics;

namespace NpcMovementLib.Math;

public struct Vec3 : IEquatable<Vec3>
{
    public double X;
    public double Y;
    public double Z;

    public Vec3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vec3 Zero => new(0, 0, 0);

    public readonly double Size()
    {
        return System.Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    public readonly Vec3 NormalizeSafe()
    {
        var magnitude = Size();
        const double threshold = 1e-6;

        if (magnitude > threshold)
        {
            return new Vec3(X / magnitude, Y / magnitude, Z / magnitude);
        }

        return Zero;
    }

    public readonly Vec3 Normalized()
    {
        var magnitude = Size();
        if (magnitude < 1e-10) return Zero;
        return new Vec3(X / magnitude, Y / magnitude, Z / magnitude);
    }

    public readonly Vec3 ClampToSize(double maxLength)
    {
        var length = Size();
        if (length > maxLength)
        {
            return Normalized() * maxLength;
        }

        return this;
    }

    public readonly double Dot(Vec3 other)
    {
        return X * other.X + Y * other.Y + Z * other.Z;
    }

    public readonly Vec3 CrossProduct(Vec3 other)
    {
        return new Vec3(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );
    }

    public readonly Vec3 Reverse()
    {
        return new Vec3(-X, -Y, -Z);
    }

    public readonly double Dist(Vec3 other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public readonly Vector3 ToVector3()
    {
        return new Vector3((float)X, (float)Y, (float)Z);
    }

    public static Vec3 FromVector3(Vector3 v)
    {
        return new Vec3(v.X, v.Y, v.Z);
    }

    public readonly Vec3 DividedBy(double value)
    {
        if (value == 0) throw new DivideByZeroException("Cannot divide by zero.");
        return new Vec3(X / value, Y / value, Z / value);
    }

    public static Vec3 Lerp(Vec3 start, Vec3 end, double speed, double deltaTime)
    {
        var moveAmount = speed * deltaTime;
        var direction = end - start;
        var distance = direction.Size();

        if (distance < 1e-10) return end;

        moveAmount = System.Math.Min(moveAmount, distance);

        return new Vec3(
            start.X + direction.X * moveAmount / distance,
            start.Y + direction.Y * moveAmount / distance,
            start.Z + direction.Z * moveAmount / distance
        );
    }

    // Operators
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator *(Vec3 v, double s) => new(v.X * s, v.Y * s, v.Z * s);
    public static Vec3 operator *(double s, Vec3 v) => new(v.X * s, v.Y * s, v.Z * s);
    public static Vec3 operator /(Vec3 v, double s) => new(v.X / s, v.Y / s, v.Z / s);
    public static Vec3 operator -(Vec3 v) => new(-v.X, -v.Y, -v.Z);

    public static bool operator ==(Vec3 a, Vec3 b) => a.Equals(b);
    public static bool operator !=(Vec3 a, Vec3 b) => !a.Equals(b);

    public readonly bool Equals(Vec3 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    public override readonly bool Equals(object? obj) => obj is Vec3 other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override readonly string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
}
