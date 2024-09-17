using System;
using System.Numerics;
using MathNet.Spatial.Euclidean;
using NQ;
using Quaternion = System.Numerics.Quaternion;

namespace Mod.DynamicEncounters.Helpers;

public static class VectorMathUtils
{
    // Function to calculate the rotation quaternion to align with a given direction vector
    public static Quaternion SetRotationToMatchDirection(Vector3 currentPosition, Vector3 targetPosition)
    {
        // Calculate the direction vector from current position to the target position
        var direction = targetPosition - currentPosition;
        
        // Ensure the direction vector is not zero-length
        if (direction.LengthSquared() < 1e-6f)
        {
            return Quaternion.Identity; // No rotation needed
        }

        // Normalize direction vector
        direction = Vector3.Normalize(direction);

        // Define world forward vector (Y-axis aligned)
        var worldForward = new Vector3(0, 1, 0);

        // Check if the direction is exactly aligned with the forward vector
        if (Vector3.Dot(direction, worldForward) > 0.9999f)
        {
            return Quaternion.Identity; // No rotation needed
        }

        // Calculate the axis of rotation (cross product of the world forward and the target direction)
        var rotationAxis = Vector3.Cross(worldForward, direction);

        // Handle edge case where the axis of rotation is zero-length (parallel vectors)
        if (rotationAxis.LengthSquared() < 1e-6f)
        {
            // If axis is zero-length, vectors are anti-parallel
            // We need a 180-degree rotation around any perpendicular axis
            rotationAxis = new Vector3(1, 0, 0); // Arbitrarily chosen
        }
        else
        {
            rotationAxis = Vector3.Normalize(rotationAxis);
        }

        // Calculate the angle between the world forward and the direction vector
        var dotProduct = Vector3.Dot(worldForward, direction);
        var angle = MathF.Acos(Math.Clamp(dotProduct, -1.0f, 1.0f));

        // Create the quaternion from the axis and angle of rotation
        return Quaternion.CreateFromAxisAngle(rotationAxis, angle);
    }

    // Function to apply the quaternion to the current entity's rotation
    public static Quaternion ApplyRotation(Quaternion currentRotation, Quaternion targetRotation)
    {
        // Combine current rotation with the new calculated rotation (multiplying quaternions)
        return Quaternion.Normalize(Quaternion.Concatenate(currentRotation, targetRotation));
    }
    
    public static Vector3 GetLocalForward(this Quaternion quaternion)
    {
        // World forward vector (assuming it's +Y axis in world coordinates)
        var worldForward = new Vector3(0, 1, 0);

        return quaternion.GetLocal(worldForward);
    }
    
    public static Vector3 GetLocalRight(this Quaternion quaternion)
    {
        // World forward vector (assuming it's +Y axis in world coordinates)
        var worldForward = new Vector3(1, 0, 0);

        return quaternion.GetLocal(worldForward);
    }
    
    public static Vector3 GetLocalUp(this Quaternion quaternion)
    {
        // World forward vector (assuming it's +Y axis in world coordinates)
        var worldForward = new Vector3(0, 0, 1);

        return quaternion.GetLocal(worldForward);
    }
    
    public static Vector3 GetLocal(this Quaternion quaternion, Vector3 worldDirection)
    {
        // Inverse the quaternion (world rotation of the object)
        var inverseRotation = quaternion.ToNqQuat().ToMnQuat().Inversed;

        // Apply the inverse rotation to the world forward vector to get the local forward vector
        var r =  inverseRotation.Rotate(new Vector3D(worldDirection.X, worldDirection.Y, worldDirection.Z));

        return new Vector3((float)r.X, (float)r.Y, (float)r.Z);
    }
}