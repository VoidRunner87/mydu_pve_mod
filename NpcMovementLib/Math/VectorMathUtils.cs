using System.Numerics;

namespace NpcMovementLib.Math;

/// <summary>
/// Provides utility methods for 3D rotation and coordinate-space transformations using
/// <see cref="System.Numerics"/> types.
/// </summary>
/// <remarks>
/// These helpers are used by the NPC movement system to orient constructs toward targets,
/// convert between world and local coordinate spaces, and extract forward directions from
/// quaternion rotations. The engine convention uses the positive Y axis as the "forward"
/// direction.
/// </remarks>
public static class VectorMathUtils
{
    /// <summary>
    /// Computes the quaternion rotation that orients the forward direction (positive Y axis)
    /// to face from <paramref name="currentPosition"/> toward <paramref name="targetPosition"/>.
    /// </summary>
    /// <param name="currentPosition">The world-space position of the entity to rotate.</param>
    /// <param name="targetPosition">The world-space position the entity should face.</param>
    /// <returns>
    /// A normalized <see cref="Quaternion"/> that, when applied to a construct, aligns its forward
    /// (Y) axis with the direction toward <paramref name="targetPosition"/>.
    /// Returns <see cref="Quaternion.Identity"/> if the two positions are nearly coincident
    /// (squared distance &lt; <c>1e-6</c>) or the direction already matches the forward axis.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The rotation is computed via axis-angle: the axis is the cross product of the world forward
    /// <c>(0, 1, 0)</c> and the target direction, and the angle is the arc-cosine of their dot product.
    /// </para>
    /// <para>
    /// Edge cases handled:
    /// <list type="bullet">
    ///   <item>Near-zero direction vector (coincident positions): returns <see cref="Quaternion.Identity"/>.</item>
    ///   <item>Direction nearly parallel to forward (dot &gt; 0.9999): returns <see cref="Quaternion.Identity"/>.</item>
    ///   <item>Direction anti-parallel to forward (cross product near-zero): uses the X axis <c>(1, 0, 0)</c>
    ///         as an arbitrary rotation axis for the 180-degree rotation.</item>
    /// </list>
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// Returns the forward direction vector for a given rotation by transforming the world forward
    /// axis <c>(0, 1, 0)</c>.
    /// </summary>
    /// <param name="rotation">The orientation quaternion of the entity.</param>
    /// <returns>
    /// A <see cref="Vector3"/> representing the direction the entity is facing in world space.
    /// The result is unit-length if <paramref name="rotation"/> is normalized.
    /// </returns>
    /// <remarks>
    /// This engine uses Y-forward convention, so the "forward" vector is <c>(0, 1, 0)</c>
    /// rather than the more common <c>(0, 0, -1)</c> or <c>(0, 0, 1)</c>.
    /// </remarks>
    public static Vector3 GetForward(Quaternion rotation)
    {
        var forward = new Vector3(0, 1, 0);
        return Vector3.Transform(forward, rotation);
    }

    /// <summary>
    /// Combines two rotations and returns the normalized result.
    /// </summary>
    /// <param name="currentRotation">The existing rotation of the entity.</param>
    /// <param name="targetRotation">The additional rotation to apply on top of the current rotation.</param>
    /// <returns>
    /// A normalized <see cref="Quaternion"/> representing the combined rotation.
    /// </returns>
    /// <remarks>
    /// Uses <see cref="Quaternion.Concatenate"/> which applies <paramref name="targetRotation"/>
    /// after <paramref name="currentRotation"/>. The result is normalized to prevent drift
    /// from accumulated floating-point errors over many frames.
    /// </remarks>
    public static Quaternion ApplyRotation(Quaternion currentRotation, Quaternion targetRotation)
    {
        return Quaternion.Normalize(Quaternion.Concatenate(currentRotation, targetRotation));
    }

    /// <summary>
    /// Transforms a world-space position into the local coordinate space of a reference object.
    /// </summary>
    /// <param name="targetPosition">The world-space position to transform (e.g., Ship A).</param>
    /// <param name="referencePosition">The world-space position of the reference frame origin (e.g., Ship B).</param>
    /// <param name="referenceRotation">The world-space rotation of the reference frame (e.g., Ship B's orientation).</param>
    /// <returns>
    /// The position of <paramref name="targetPosition"/> expressed in <paramref name="referencePosition"/>'s
    /// local coordinate space.
    /// </returns>
    /// <remarks>
    /// The transformation is performed in two steps:
    /// <list type="number">
    ///   <item>Translate: subtract the reference position to get a world-relative offset.</item>
    ///   <item>Rotate: apply the inverse of the reference rotation to express the offset in local space.</item>
    /// </list>
    /// This is the inverse of <see cref="CalculateWorldPosition"/>.
    /// </remarks>
    public static Vector3 CalculateRelativePosition(Vector3 targetPosition, Vector3 referencePosition,
        Quaternion referenceRotation)
    {
        var relativePosition = targetPosition - referencePosition;
        var inverseRotation = Quaternion.Inverse(referenceRotation);
        return Vector3.Transform(relativePosition, inverseRotation);
    }

    /// <summary>
    /// Transforms a local-space position back into world space using a reference object's pose.
    /// </summary>
    /// <param name="relativePosition">The position in the reference object's local coordinate space.</param>
    /// <param name="referencePosition">The world-space position of the reference frame origin.</param>
    /// <param name="referenceRotation">The world-space rotation of the reference frame.</param>
    /// <returns>The world-space position corresponding to <paramref name="relativePosition"/>.</returns>
    /// <remarks>
    /// The transformation is performed in two steps:
    /// <list type="number">
    ///   <item>Rotate: apply the reference rotation to orient the local offset into world space.</item>
    ///   <item>Translate: add the reference position to produce the final world coordinate.</item>
    /// </list>
    /// This is the inverse of <see cref="CalculateRelativePosition"/>.
    /// </remarks>
    public static Vector3 CalculateWorldPosition(Vector3 relativePosition, Vector3 referencePosition,
        Quaternion referenceRotation)
    {
        var rotatedPosition = Vector3.Transform(relativePosition, referenceRotation);
        return rotatedPosition + referencePosition;
    }
}
