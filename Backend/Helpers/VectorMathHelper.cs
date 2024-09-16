using System;
using System.Numerics;
using NQ;

namespace Mod.DynamicEncounters.Helpers;

public static class VectorMathHelper
{
    public static Vec3 DividedBy(this Vec3 vec, double value)
    {
        if (value == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero.");
        }

        return new Vec3
        {
            x = vec.x / value,
            y = vec.y / value,
            z = vec.z / value
        };
    }
    
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
    
    public static Vec3 CrossProduct(this Vec3 v1, Vec3 v2)
    {
        return new Vec3
        {
            x = v1.y * v2.z - v1.z * v2.y,
            y = v1.z * v2.x - v1.x * v2.z,
            z = v1.x * v2.y - v1.y * v2.x
        };
    }
    
    public static Vec3 Reverse(this Vec3 vec)
    {
        return new Vec3
        {
            x = -vec.x,
            y = -vec.y,
            z = -vec.z
        };
    }
    
    public static Vec3 CalculateAngularVelocity(Vec3 d0, Vec3 d, double deltaTime)
    {
        if (deltaTime <= 0)
        {
            return d;
        }
        
        Vec3 d0Normalized = d0.Normalized();
        Vec3 dNormalized = d.Normalized();

        Vec3 rotationAxis = d0Normalized.CrossProduct(dNormalized);
        double dot = d0Normalized.Dot(dNormalized);

        double angle = Math.Acos(dot); // Angle in radians
        double angularVelocityMagnitude = angle / deltaTime;

        return new Vec3
        {
            x = rotationAxis.x * angularVelocityMagnitude,
            y = rotationAxis.y * angularVelocityMagnitude,
            z = rotationAxis.z * angularVelocityMagnitude
        };
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
    
    public static Vec3 NormalizeSafe(this Vec3 v)
    {
        var magnitude = v.Size();
        const double threshold = 1e-6; // A small value to avoid division by zero

        if (magnitude > threshold)
        {
            return new Vec3
            {
                x = v.x / magnitude,
                y = v.y / magnitude,
                z = v.z / magnitude
            };
        }

        // Return zero vector if magnitude is too small
        return new Vec3 { x = 0, y = 0, z = 0 };
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
    
    public static double Dot(this Vec3 v1, Vec3 v2)
    {
        return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
    }
    
    public static Vec3 Lerp(Vec3 start, Vec3 end, double speed, double deltaTime)
    {
        // Calculate the maximum amount to move in this frame
        var moveAmount = speed * deltaTime;

        // Calculate the direction vector from start to end
        var direction = new Vec3
        {
            x = end.x - start.x,
            y = end.y - start.y,
            z = end.z - start.z
        };
        var distance = Math.Sqrt(direction.x * direction.x + direction.y * direction.y + direction.z * direction.z);

        // Clamp the move amount to the distance to avoid overshooting
        moveAmount = Math.Min(moveAmount, distance);

        // Calculate the new position
        var result = new Vec3
        {
            x = start.x + direction.x * moveAmount / distance,
            y = start.y + direction.y * moveAmount / distance,
            z = start.z + direction.z * moveAmount / distance
        };

        return result;
    }
}