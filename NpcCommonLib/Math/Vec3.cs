using System.Numerics;

namespace NpcCommonLib.Math;

/// <summary>
/// A double-precision 3D vector used for NPC position, velocity, and acceleration calculations.
/// </summary>
/// <remarks>
/// This struct mirrors the game's <c>NQ.Vec3</c> type but uses double-precision fields
/// (<see cref="X"/>, <see cref="Y"/>, <see cref="Z"/>) for accurate large-scale space simulation.
/// It implements <see cref="IEquatable{Vec3}"/> for value-based equality comparisons.
/// </remarks>
public struct Vec3 : IEquatable<Vec3>
{
    /// <summary>The X component of the vector (typically metres in world space).</summary>
    public double X;

    /// <summary>The Y component of the vector (typically metres in world space). In this engine the Y axis is the default forward direction.</summary>
    public double Y;

    /// <summary>The Z component of the vector (typically metres in world space).</summary>
    public double Z;

    /// <summary>
    /// Initializes a new <see cref="Vec3"/> with the specified components.
    /// </summary>
    /// <param name="x">The X component.</param>
    /// <param name="y">The Y component.</param>
    /// <param name="z">The Z component.</param>
    public Vec3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Gets a <see cref="Vec3"/> with all components set to zero.
    /// </summary>
    public static Vec3 Zero => new(0, 0, 0);

    /// <summary>
    /// Returns the Euclidean length (magnitude) of this vector.
    /// </summary>
    /// <returns>The scalar length in the same units as the vector components (typically metres).</returns>
    /// <remarks>
    /// Uses <c>Math.Sqrt(X*X + Y*Y + Z*Z)</c>. For distance comparisons where the
    /// actual magnitude is not needed, prefer comparing squared lengths to avoid the sqrt cost.
    /// </remarks>
    public readonly double Size()
    {
        return System.Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    /// <summary>
    /// Returns a unit-length vector in the same direction, or <see cref="Zero"/> if the magnitude
    /// is below a safety threshold.
    /// </summary>
    /// <returns>
    /// A normalized copy of this vector, or <see cref="Zero"/> when the magnitude is less than or
    /// equal to <c>1e-6</c>.
    /// </returns>
    /// <remarks>
    /// This is the preferred normalization method when the vector might be zero-length or
    /// near-zero (e.g., when computing direction from two nearly coincident positions).
    /// The threshold of <c>1e-6</c> avoids floating-point instability from dividing by
    /// a very small number. Used extensively in movement effects (e.g., clamping delta-V direction).
    /// </remarks>
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

    /// <summary>
    /// Returns a unit-length vector in the same direction, or <see cref="Zero"/> if the magnitude
    /// is below <c>1e-10</c>.
    /// </summary>
    /// <returns>A normalized copy of this vector.</returns>
    /// <remarks>
    /// Similar to <see cref="NormalizeSafe"/> but uses a tighter threshold of <c>1e-10</c>.
    /// Prefer <see cref="NormalizeSafe"/> when the source vector may be legitimately near-zero;
    /// use this variant when a stricter near-zero check is acceptable.
    /// </remarks>
    public readonly Vec3 Normalized()
    {
        var magnitude = Size();
        if (magnitude < 1e-10) return Zero;
        return new Vec3(X / magnitude, Y / magnitude, Z / magnitude);
    }

    /// <summary>
    /// Returns this vector clamped so its magnitude does not exceed <paramref name="maxLength"/>.
    /// </summary>
    /// <param name="maxLength">The maximum allowed magnitude (e.g., maximum speed in m/s).</param>
    /// <returns>
    /// If the vector's length exceeds <paramref name="maxLength"/>, a vector with the same direction
    /// scaled to <paramref name="maxLength"/>; otherwise, the original vector unchanged.
    /// </returns>
    /// <remarks>
    /// Used in the movement simulation to enforce velocity caps after acceleration is applied.
    /// For example, <c>velocity.ClampToSize(maxSpeed)</c> ensures the NPC never exceeds its top speed.
    /// </remarks>
    public readonly Vec3 ClampToSize(double maxLength)
    {
        var length = Size();
        if (length > maxLength)
        {
            return Normalized() * maxLength;
        }

        return this;
    }

    /// <summary>
    /// Computes the dot product of this vector with <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The second operand vector.</param>
    /// <returns>The scalar dot product <c>X*other.X + Y*other.Y + Z*other.Z</c>.</returns>
    /// <remarks>
    /// The dot product is positive when vectors point in a similar direction, zero when perpendicular,
    /// and negative when opposing. Used to compute relative speed along a line-of-sight direction
    /// (e.g., in <see cref="VelocityHelper.CalculateTimeToReachDistance"/>).
    /// </remarks>
    public readonly double Dot(Vec3 other)
    {
        return X * other.X + Y * other.Y + Z * other.Z;
    }

    /// <summary>
    /// Computes the cross product of this vector with <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The second operand vector.</param>
    /// <returns>
    /// A new <see cref="Vec3"/> perpendicular to both input vectors, with magnitude equal to the
    /// area of the parallelogram they span.
    /// </returns>
    /// <remarks>
    /// The result follows the right-hand rule. The cross product is commonly used to find
    /// rotation axes or surface normals.
    /// </remarks>
    public readonly Vec3 CrossProduct(Vec3 other)
    {
        return new Vec3(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );
    }

    /// <summary>
    /// Returns the negation of this vector (each component multiplied by -1).
    /// </summary>
    /// <returns>A new <see cref="Vec3"/> pointing in the opposite direction with the same magnitude.</returns>
    public readonly Vec3 Reverse()
    {
        return new Vec3(-X, -Y, -Z);
    }

    /// <summary>
    /// Computes the Euclidean distance between this point and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The target point.</param>
    /// <returns>The straight-line distance between the two points in the same units as the components (typically metres).</returns>
    public readonly double Dist(Vec3 other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Converts this double-precision vector to a <see cref="Vector3"/> (single-precision).
    /// </summary>
    /// <returns>A <see cref="Vector3"/> with each component cast to <see langword="float"/>.</returns>
    /// <remarks>
    /// Used when interacting with <see cref="System.Numerics"/> APIs (e.g., quaternion rotation)
    /// that require single-precision vectors. Precision loss is expected for very large coordinates.
    /// </remarks>
    public readonly Vector3 ToVector3()
    {
        return new Vector3((float)X, (float)Y, (float)Z);
    }

    /// <summary>
    /// Creates a <see cref="Vec3"/> from a <see cref="Vector3"/> (single-precision to double-precision).
    /// </summary>
    /// <param name="v">The source <see cref="Vector3"/>.</param>
    /// <returns>A new <see cref="Vec3"/> with each component promoted to <see langword="double"/>.</returns>
    public static Vec3 FromVector3(Vector3 v)
    {
        return new Vec3(v.X, v.Y, v.Z);
    }

    /// <summary>
    /// Divides each component of this vector by a scalar value.
    /// </summary>
    /// <param name="value">The scalar divisor. Must not be zero.</param>
    /// <returns>A new <see cref="Vec3"/> with each component divided by <paramref name="value"/>.</returns>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="value"/> is exactly zero.</exception>
    public readonly Vec3 DividedBy(double value)
    {
        if (value == 0) throw new DivideByZeroException("Cannot divide by zero.");
        return new Vec3(X / value, Y / value, Z / value);
    }

    /// <summary>
    /// Moves a point from <paramref name="start"/> toward <paramref name="end"/> at a fixed speed,
    /// clamped so it never overshoots the destination.
    /// </summary>
    /// <param name="start">The current position.</param>
    /// <param name="end">The target position.</param>
    /// <param name="speed">Movement speed in units per second (e.g., m/s).</param>
    /// <param name="deltaTime">The elapsed time for this simulation step, in seconds.</param>
    /// <returns>
    /// The new position after moving toward <paramref name="end"/>. Returns <paramref name="end"/>
    /// directly if the remaining distance is smaller than the movement amount for this frame,
    /// or if the distance is near-zero (<c>&lt; 1e-10</c>).
    /// </returns>
    /// <remarks>
    /// Despite the name, this is not a true linear interpolation (blend factor between 0 and 1).
    /// Instead it performs a constant-speed "move toward" operation: the object advances by
    /// <c>speed * deltaTime</c> each frame along the direction from <paramref name="start"/>
    /// to <paramref name="end"/>.
    /// </remarks>
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

    /// <summary>
    /// Adds two vectors component-wise.
    /// </summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns>A new <see cref="Vec3"/> where each component is the sum of the corresponding components of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    /// <summary>
    /// Subtracts vector <paramref name="b"/> from vector <paramref name="a"/> component-wise.
    /// </summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns>A new <see cref="Vec3"/> where each component is the difference of the corresponding components.</returns>
    /// <remarks>
    /// Commonly used to compute direction vectors: <c>target - current</c> yields the displacement
    /// from <c>current</c> to <c>target</c>.
    /// </remarks>
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    /// <summary>
    /// Multiplies each component of vector <paramref name="v"/> by scalar <paramref name="s"/>.
    /// </summary>
    /// <param name="v">The vector operand.</param>
    /// <param name="s">The scalar multiplier (e.g., speed, deltaTime).</param>
    /// <returns>A new <see cref="Vec3"/> scaled by <paramref name="s"/>.</returns>
    public static Vec3 operator *(Vec3 v, double s) => new(v.X * s, v.Y * s, v.Z * s);

    /// <summary>
    /// Multiplies each component of vector <paramref name="v"/> by scalar <paramref name="s"/>.
    /// </summary>
    /// <param name="s">The scalar multiplier.</param>
    /// <param name="v">The vector operand.</param>
    /// <returns>A new <see cref="Vec3"/> scaled by <paramref name="s"/>.</returns>
    /// <remarks>Commutative counterpart to <c>Vec3 * double</c>.</remarks>
    public static Vec3 operator *(double s, Vec3 v) => new(v.X * s, v.Y * s, v.Z * s);

    /// <summary>
    /// Divides each component of vector <paramref name="v"/> by scalar <paramref name="s"/>.
    /// </summary>
    /// <param name="v">The vector operand.</param>
    /// <param name="s">The scalar divisor.</param>
    /// <returns>A new <see cref="Vec3"/> with each component divided by <paramref name="s"/>.</returns>
    /// <remarks>
    /// Unlike <see cref="DividedBy"/>, this operator does not guard against division by zero.
    /// Dividing by zero will produce <see cref="double.PositiveInfinity"/> or <see cref="double.NaN"/>.
    /// </remarks>
    public static Vec3 operator /(Vec3 v, double s) => new(v.X / s, v.Y / s, v.Z / s);

    /// <summary>
    /// Negates all components of the vector (equivalent to <see cref="Reverse"/>).
    /// </summary>
    /// <param name="v">The vector to negate.</param>
    /// <returns>A new <see cref="Vec3"/> pointing in the opposite direction.</returns>
    public static Vec3 operator -(Vec3 v) => new(-v.X, -v.Y, -v.Z);

    /// <summary>
    /// Determines whether two <see cref="Vec3"/> instances are equal (exact component-wise comparison).
    /// </summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns><see langword="true"/> if all components are exactly equal; otherwise <see langword="false"/>.</returns>
    /// <remarks>Uses <see cref="double.Equals(double)"/> per component, which is an exact bit-wise comparison.</remarks>
    public static bool operator ==(Vec3 a, Vec3 b) => a.Equals(b);

    /// <summary>
    /// Determines whether two <see cref="Vec3"/> instances are not equal.
    /// </summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns><see langword="true"/> if any component differs; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(Vec3 a, Vec3 b) => !a.Equals(b);

    /// <summary>
    /// Determines whether this vector is exactly equal to <paramref name="other"/> (component-wise).
    /// </summary>
    /// <param name="other">The vector to compare with.</param>
    /// <returns><see langword="true"/> if all components match exactly; otherwise <see langword="false"/>.</returns>
    public readonly bool Equals(Vec3 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) => obj is Vec3 other && Equals(other);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    /// <summary>
    /// Returns a human-readable string representation of the vector in the format <c>(X, Y, Z)</c>
    /// with two decimal places.
    /// </summary>
    /// <returns>A formatted string such as <c>(1.00, 2.50, -3.14)</c>.</returns>
    public override readonly string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
}
