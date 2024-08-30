using System;
using System.Numerics;
using NQ;

namespace Mod.DynamicEncounters.Helpers;

public static class VectorMathHelper
{
    public static Vec3 GridSnap(this Vec3 input, double snapValue)
    {
        var v = input;

        v.x = Math.Round(v.x / snapValue) * snapValue;
        v.y = Math.Round(v.y / snapValue) * snapValue;
        v.z = Math.Round(v.z / snapValue) * snapValue;

        return v;
    }

    public static Vec3 Clamp(this Vec3 vector, Vec3 min, Vec3 max)
    {
        return new Vec3
        {
            x = Math.Clamp(vector.x, min.x, max.x),
            y = Math.Clamp(vector.y, min.y, max.y),
            z = Math.Clamp(vector.z, min.z, max.z)
        };
    }

    public static double Distance(this Vec3 v1, Vec3 v2)
    {
        var deltaX = v1.x - v2.x;
        var deltaY = v1.y - v2.y;
        var deltaZ = v1.z - v2.z;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
    }

    public static double Size(this Vec3 vector)
    {
        return Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }

    public static Vec3 ClampToSize(this Vec3 vector, double maxLength)
    {
        var length = vector.Size();

        if (length > maxLength)
        {
            // Scale the vector to the max length
            vector = vector.Normalized() * maxLength;
        }

        return vector;
    }

    private static Vector3 ToVector3(this Vec3 v)
    {
        return new Vector3((float)v.x, (float)v.y, (float)v.z);
    }

    public static Quat CalculateRotationToPoint(Vec3 currentPosition, Vec3 targetPosition)
    {
        var currentVec = currentPosition.ToVector3();
        var targetVec = targetPosition.ToVector3();

        // Calculate the forward direction for the first ship (assumed to be along the positive y-axis)
        var forward = new Vector3(0, 1, 0);

        // Calculate the direction to the target
        var direction = Vector3.Normalize(targetVec - currentVec);

        // Calculate the quaternion that rotates the forward direction to the target direction
        var rotation = QuaternionFromTo(forward, direction);

        // Convert System.Numerics.Quaternion to Quat
        return FromQuaternion(rotation);
    }

    private static Quaternion QuaternionFromTo(Vector3 from, Vector3 to)
    {
        // Calculate the cross product and dot product
        var cross = Vector3.Cross(from, to);
        var dot = Vector3.Dot(from, to);

        // Calculate the quaternion components
        var angle = MathF.Acos(dot);
        var s = MathF.Sin(angle / 2);

        var quaternion = new Quaternion(
            cross.X * s,
            cross.Y * s,
            cross.Z * s,
            MathF.Cos(angle / 2)
        );

        return Quaternion.Normalize(quaternion);
    }

    public static Quaternion ToQuaternion(this Quat q) => new(q.x, q.y, q.z, q.w);

    public static Quat FromQuaternion(this Quaternion q)
    {
        return Quat.FromComponents(q.W, new Vec3 { x = q.X, y = q.Y, z = q.Z });
    }
}