using System.Numerics;

namespace NpcMovementLib.Math;

public static class VectorMathUtils
{
    public static Quaternion SetRotationToMatchDirection(Vector3 currentPosition, Vector3 targetPosition)
    {
        var direction = targetPosition - currentPosition;

        if (direction.LengthSquared() < 1e-6f)
        {
            return Quaternion.Identity;
        }

        direction = Vector3.Normalize(direction);

        var worldForward = new Vector3(0, 1, 0);

        if (Vector3.Dot(direction, worldForward) > 0.9999f)
        {
            return Quaternion.Identity;
        }

        var rotationAxis = Vector3.Cross(worldForward, direction);

        if (rotationAxis.LengthSquared() < 1e-6f)
        {
            rotationAxis = new Vector3(1, 0, 0);
        }
        else
        {
            rotationAxis = Vector3.Normalize(rotationAxis);
        }

        var dotProduct = Vector3.Dot(worldForward, direction);
        var angle = MathF.Acos(System.Math.Clamp(dotProduct, -1.0f, 1.0f));

        return Quaternion.CreateFromAxisAngle(rotationAxis, angle);
    }

    public static Vector3 GetForward(Quaternion rotation)
    {
        var forward = new Vector3(0, 1, 0);
        return Vector3.Transform(forward, rotation);
    }

    public static Quaternion ApplyRotation(Quaternion currentRotation, Quaternion targetRotation)
    {
        return Quaternion.Normalize(Quaternion.Concatenate(currentRotation, targetRotation));
    }

    public static Vector3 CalculateRelativePosition(Vector3 targetPosition, Vector3 referencePosition,
        Quaternion referenceRotation)
    {
        var relativePosition = targetPosition - referencePosition;
        var inverseRotation = Quaternion.Inverse(referenceRotation);
        return Vector3.Transform(relativePosition, inverseRotation);
    }

    public static Vector3 CalculateWorldPosition(Vector3 relativePosition, Vector3 referencePosition,
        Quaternion referenceRotation)
    {
        var rotatedPosition = Vector3.Transform(relativePosition, referenceRotation);
        return rotatedPosition + referencePosition;
    }
}
