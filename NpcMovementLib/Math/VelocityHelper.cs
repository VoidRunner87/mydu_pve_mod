using System.Numerics;
using NpcCommonLib.Math;

namespace NpcMovementLib.Math;

/// <summary>
/// Provides physics-based helper methods for NPC movement simulation, including kinematic
/// interpolation with acceleration, braking, and trajectory prediction.
/// </summary>
/// <remarks>
/// These methods implement Euler and Verlet-style numerical integration for moving NPC constructs
/// along paths. They are called each simulation tick by movement effects such as
/// "burn to target" and "apply brakes" behaviors. All methods use double-precision arithmetic
/// via <see cref="Vec3"/> for positional accuracy in large game worlds.
/// </remarks>
public static class VelocityHelper
{
    /// <summary>
    /// Calculates the distance required to decelerate from a given velocity to zero
    /// under constant deceleration.
    /// </summary>
    /// <param name="velocity">The current scalar speed in m/s.</param>
    /// <param name="deceleration">The constant deceleration magnitude in m/s² (must be positive).</param>
    /// <returns>The braking distance in metres, computed as <c>v² / (2 * a)</c>.</returns>
    /// <remarks>
    /// Derived from the kinematic equation <c>v² = v₀² + 2a·d</c> with final velocity = 0.
    /// Used by <see cref="ShouldStartBraking"/> to decide when an NPC should begin decelerating.
    /// </remarks>
    public static double CalculateBrakingDistance(double velocity, double deceleration)
    {
        return System.Math.Pow(velocity, 2) / (2 * deceleration);
    }

    /// <summary>
    /// Calculates the time required to decelerate from <paramref name="initialVelocity"/> to zero
    /// under constant deceleration.
    /// </summary>
    /// <param name="initialVelocity">The starting speed in m/s.</param>
    /// <param name="deceleration">The deceleration magnitude in m/s² (must be positive for meaningful results).</param>
    /// <returns>
    /// The braking time in seconds (<c>v₀ / a</c>). Returns <c>3600</c> (one hour) if
    /// <paramref name="deceleration"/> is zero or negative (representing "effectively never").
    /// Returns <c>0</c> if <paramref name="initialVelocity"/> is zero or negative.
    /// </returns>
    public static double CalculateBrakingTime(double initialVelocity, double deceleration)
    {
        if (deceleration <= 0) return 60 * 60;
        if (initialVelocity <= 0) return 0;
        return initialVelocity / deceleration;
    }

    /// <summary>
    /// Determines whether the NPC should begin braking to stop at the target position.
    /// </summary>
    /// <param name="currentPosition">The NPC's current world-space position.</param>
    /// <param name="targetPosition">The destination world-space position.</param>
    /// <param name="currentVelocity">The NPC's current velocity vector.</param>
    /// <param name="deceleration">The available deceleration magnitude in m/s².</param>
    /// <returns>
    /// <see langword="true"/> if the remaining distance to the target is less than or equal to the
    /// braking distance at the current speed; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Compares the straight-line remaining distance against the kinematic braking distance
    /// (<see cref="CalculateBrakingDistance"/>). When this returns <see langword="true"/>,
    /// the movement system typically switches from "burn to target" to "apply brakes" behavior.
    /// </remarks>
    public static bool ShouldStartBraking(Vector3 currentPosition, Vector3 targetPosition,
        Vector3 currentVelocity, double deceleration)
    {
        double remainingDistance = Vector3.Distance(currentPosition, targetPosition);
        var brakingDistance = CalculateBrakingDistance(currentVelocity.Length(), deceleration);
        return remainingDistance <= brakingDistance;
    }

    /// <summary>
    /// Calculates the time needed to change speed from <paramref name="initialVelocity"/> to
    /// <paramref name="targetVelocity"/> at a given acceleration.
    /// </summary>
    /// <param name="initialVelocity">The starting speed in m/s.</param>
    /// <param name="targetVelocity">The desired speed in m/s.</param>
    /// <param name="acceleration">The acceleration magnitude in m/s².</param>
    /// <returns>
    /// The absolute time in seconds. Returns <c>3600</c> (one hour) if
    /// <paramref name="acceleration"/> is zero (representing "effectively never").
    /// </returns>
    public static double CalculateTimeToReachVelocity(
        double initialVelocity, double targetVelocity, double acceleration)
    {
        if (acceleration == 0) return 60 * 60;
        var time = (targetVelocity - initialVelocity) / acceleration;
        return System.Math.Abs(time);
    }

    /// <summary>
    /// Advances a position from <paramref name="start"/> toward <paramref name="end"/> using
    /// velocity-Verlet integration with acceleration, updating velocity by reference.
    /// </summary>
    /// <param name="start">The current position of the NPC.</param>
    /// <param name="end">The target position the NPC is moving toward.</param>
    /// <param name="velocity">
    /// The current velocity vector, passed by reference. Updated in place after Euler integration
    /// (<c>v += a * dt</c>) and then clamped to <paramref name="clampSize"/>.
    /// </param>
    /// <param name="acceleration">The acceleration vector applied this tick (direction and magnitude).</param>
    /// <param name="clampSize">Maximum allowed velocity magnitude in m/s (speed cap).</param>
    /// <param name="deltaTime">Simulation time step in seconds.</param>
    /// <param name="handleOvershoot">
    /// When <see langword="true"/>, if the computed displacement exceeds the distance to
    /// <paramref name="end"/>, the position snaps to <paramref name="end"/> and velocity is zeroed.
    /// </param>
    /// <returns>
    /// The new position after integration. Returns <paramref name="end"/> if the start-to-end
    /// distance is less than <c>0.001</c> metres, or if NaN values are detected.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Uses the kinematic equation <c>s = v*t + 0.5*a*t²</c> for displacement and <c>v = v + a*t</c>
    /// for velocity update (semi-implicit Euler / Verlet). The velocity is clamped after update.
    /// </para>
    /// <para>
    /// NaN safety: if any component of the new position or velocity is NaN (e.g., from degenerate
    /// acceleration), the position is set to <paramref name="end"/>.
    /// </para>
    /// </remarks>
    public static Vec3 LinearInterpolateWithAcceleration(
        Vec3 start, Vec3 end, ref Vec3 velocity, Vec3 acceleration,
        double clampSize, double deltaTime, bool handleOvershoot = false)
    {
        var direction = end - start;
        var distance = direction.Size();

        if (distance < 0.001) return end;

        var accelFactor = 0.5d;

        var displacement = new Vec3(
            velocity.X * deltaTime + accelFactor * acceleration.X * deltaTime * deltaTime,
            velocity.Y * deltaTime + accelFactor * acceleration.Y * deltaTime * deltaTime,
            velocity.Z * deltaTime + accelFactor * acceleration.Z * deltaTime * deltaTime
        );

        velocity = new Vec3(
            velocity.X + acceleration.X * deltaTime,
            velocity.Y + acceleration.Y * deltaTime,
            velocity.Z + acceleration.Z * deltaTime
        );

        velocity = velocity.ClampToSize(clampSize);

        var newPosition = start + displacement;

        if (double.IsNaN(newPosition.X) || double.IsNaN(newPosition.Y) || double.IsNaN(newPosition.Z) ||
            double.IsNaN(velocity.X) || double.IsNaN(velocity.Y) || double.IsNaN(velocity.Z))
        {
            newPosition = end;
        }

        if (handleOvershoot)
        {
            if ((newPosition - start).Size() > distance)
            {
                newPosition = end;
                velocity = Vec3.Zero;
            }
        }

        return newPosition;
    }

    /// <summary>
    /// Advances a position from <paramref name="start"/> toward <paramref name="end"/> with
    /// acceleration, steering velocity magnitude toward a goal speed while maintaining the
    /// direction toward the target.
    /// </summary>
    /// <param name="start">The current position of the NPC.</param>
    /// <param name="end">The target position.</param>
    /// <param name="velocity">
    /// The current velocity vector, passed by reference. After this call, velocity is re-aligned
    /// to point toward <paramref name="end"/> and its magnitude is adjusted toward
    /// <paramref name="velocitySizeGoal"/>.
    /// </param>
    /// <param name="acceleration">
    /// The acceleration vector. Its magnitude is used as the rate of speed change per second;
    /// it is also used in the displacement formula for the Verlet position update.
    /// </param>
    /// <param name="clampSize">Hard maximum velocity magnitude in m/s.</param>
    /// <param name="velocitySizeGoal">
    /// The desired target speed in m/s. The velocity magnitude accelerates or decelerates toward
    /// this value each tick. This is typically the NPC's cruise speed or approach speed.
    /// </param>
    /// <param name="deltaTime">Simulation time step in seconds.</param>
    /// <param name="handleOvershoot">
    /// When <see langword="true"/>, snaps to <paramref name="end"/> and zeroes velocity if the
    /// displacement would overshoot.
    /// </param>
    /// <returns>
    /// The new position after integration. Returns <paramref name="end"/> if the remaining
    /// distance is less than <c>0.001</c> metres or if NaN is detected.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Unlike <see cref="LinearInterpolateWithAcceleration"/>, this version (V2) re-orients velocity
    /// to always point from <paramref name="start"/> toward <paramref name="end"/>, and adjusts the
    /// speed scalar toward <paramref name="velocitySizeGoal"/>. This produces smoother tracking of
    /// moving targets because the velocity direction is corrected every tick.
    /// </para>
    /// <para>
    /// This is the primary interpolation method used by the "burn to target" movement effect
    /// in the NPC behavior system.
    /// </para>
    /// </remarks>
    public static Vec3 LinearInterpolateWithAccelerationV2(
        Vec3 start, Vec3 end, ref Vec3 velocity, Vec3 acceleration,
        double clampSize, double velocitySizeGoal, double deltaTime,
        bool handleOvershoot = false)
    {
        var direction = end - start;
        var distance = direction.Size();

        if (distance < 0.001) return end;

        direction = direction.Normalized();

        double currentVelocitySize = velocity.Size();
        double newVelocitySize;

        if (currentVelocitySize > velocitySizeGoal)
        {
            double decelerationMagnitude = acceleration.Size();
            newVelocitySize = System.Math.Max(currentVelocitySize - decelerationMagnitude * deltaTime, velocitySizeGoal);
        }
        else
        {
            newVelocitySize = System.Math.Min(currentVelocitySize + acceleration.Size() * deltaTime, velocitySizeGoal);
        }

        velocity = direction * newVelocitySize;

        var accelFactor = 0.5d;
        var displacement = new Vec3(
            velocity.X * deltaTime + accelFactor * acceleration.X * deltaTime * deltaTime,
            velocity.Y * deltaTime + accelFactor * acceleration.Y * deltaTime * deltaTime,
            velocity.Z * deltaTime + accelFactor * acceleration.Z * deltaTime * deltaTime
        );

        var newPosition = start + displacement;

        velocity = velocity.ClampToSize(clampSize);

        if (double.IsNaN(newPosition.X) || double.IsNaN(newPosition.Y) || double.IsNaN(newPosition.Z) ||
            double.IsNaN(velocity.X) || double.IsNaN(velocity.Y) || double.IsNaN(velocity.Z))
        {
            newPosition = end;
            velocity = Vec3.Zero;
        }

        if (handleOvershoot)
        {
            if ((newPosition - start).Size() > distance)
            {
                newPosition = end;
                velocity = Vec3.Zero;
            }
        }

        return newPosition;
    }

    /// <summary>
    /// Advances a position from <paramref name="start"/> toward <paramref name="end"/> using
    /// simple Euler integration: velocity is updated by acceleration, then position is updated
    /// by velocity.
    /// </summary>
    /// <param name="start">The current position.</param>
    /// <param name="end">The target position.</param>
    /// <param name="velocity">
    /// The current velocity, passed by reference. Updated via <c>v += a * dt</c> and clamped
    /// to <paramref name="clampSize"/>.
    /// </param>
    /// <param name="acceleration">The acceleration vector applied this tick.</param>
    /// <param name="clampSize">Maximum allowed velocity magnitude in m/s.</param>
    /// <param name="deltaTime">Simulation time step in seconds.</param>
    /// <returns>
    /// The new position after integration. Snaps to <paramref name="end"/> if the remaining
    /// distance is very small (<c>&lt; 0.001</c> m) or less than the acceleration displacement
    /// for this tick, or if NaN values are detected.
    /// </returns>
    /// <remarks>
    /// This is a simpler alternative to <see cref="LinearInterpolateWithAcceleration"/> that uses
    /// first-order Euler integration (<c>p += v * dt</c>) rather than the second-order Verlet form.
    /// It does not include overshoot handling. Suitable for scenarios where smoothness of the
    /// position update is less critical.
    /// </remarks>
    public static Vec3 LinearInterpolateWithVelocity(
        Vec3 start, Vec3 end, ref Vec3 velocity, Vec3 acceleration,
        double clampSize, double deltaTime)
    {
        var direction = end - start;
        var distance = direction.Size();

        if (distance < 0.001) return end;

        velocity = new Vec3(
            velocity.X + acceleration.X * deltaTime,
            velocity.Y + acceleration.Y * deltaTime,
            velocity.Z + acceleration.Z * deltaTime
        );

        velocity = velocity.ClampToSize(clampSize);

        var newPosition = new Vec3(
            start.X + velocity.X * deltaTime,
            start.Y + velocity.Y * deltaTime,
            start.Z + velocity.Z * deltaTime
        );

        var newDirection = end - newPosition;
        var newDistance = newDirection.Size();

        if (newDistance < 0.001 || newDistance < acceleration.Size() * deltaTime)
        {
            newPosition = end;
        }

        if (double.IsNaN(newPosition.X) || double.IsNaN(newPosition.Y) || double.IsNaN(newPosition.Z) ||
            double.IsNaN(velocity.X) || double.IsNaN(velocity.Y) || double.IsNaN(velocity.Z))
        {
            newPosition = end;
        }

        return newPosition;
    }

    /// <summary>
    /// Applies per-axis braking (deceleration) to a velocity vector and returns the resulting
    /// new position after one time step.
    /// </summary>
    /// <param name="start">The current position.</param>
    /// <param name="velocity">
    /// The current velocity, passed by reference. Each axis component is independently reduced
    /// toward zero by <paramref name="decelerationRate"/> per second. If a component's absolute
    /// value is less than the per-frame deceleration, it is set to zero to prevent sign oscillation.
    /// </param>
    /// <param name="decelerationRate">
    /// The deceleration magnitude in m/s² applied independently to each axis. In the movement
    /// system, this is typically the magnitude of the NPC's acceleration vector
    /// (<c>acceleration.Size()</c>).
    /// </param>
    /// <param name="deltaTime">Simulation time step in seconds.</param>
    /// <returns>
    /// The new position after applying the braked velocity for one tick (<c>start + velocity * dt</c>).
    /// If velocity magnitude is already below <c>0.001</c> m/s, returns <paramref name="start"/>
    /// unchanged and zeroes velocity.
    /// </returns>
    /// <remarks>
    /// Unlike the interpolation methods, this does not move toward a target; it simply decelerates
    /// in place. Used by the "apply brakes" movement effect when the NPC needs to come to a stop.
    /// Braking is per-axis rather than along the velocity direction, which can slightly alter the
    /// velocity direction during deceleration.
    /// </remarks>
    public static Vec3 ApplyBraking(Vec3 start, ref Vec3 velocity, double decelerationRate, double deltaTime)
    {
        if (velocity.Size() < 0.001)
        {
            velocity = Vec3.Zero;
            return start;
        }

        var decelerationMagnitude = decelerationRate * deltaTime;

        if (System.Math.Abs(velocity.X) <= decelerationMagnitude)
            velocity.X = 0;
        else
            velocity.X += velocity.X > 0 ? -decelerationMagnitude : decelerationMagnitude;

        if (System.Math.Abs(velocity.Y) <= decelerationMagnitude)
            velocity.Y = 0;
        else
            velocity.Y += velocity.Y > 0 ? -decelerationMagnitude : decelerationMagnitude;

        if (System.Math.Abs(velocity.Z) <= decelerationMagnitude)
            velocity.Z = 0;
        else
            velocity.Z += velocity.Z > 0 ? -decelerationMagnitude : decelerationMagnitude;

        var displacement = new Vec3(
            velocity.X * deltaTime,
            velocity.Y * deltaTime,
            velocity.Z * deltaTime
        );

        return start + displacement;
    }

    /// <summary>
    /// Predicts where a position will be after <paramref name="deltaTime"/> seconds, given
    /// constant velocity and acceleration (kinematic equation).
    /// </summary>
    /// <param name="currentPosition">The starting position.</param>
    /// <param name="velocity">The current velocity vector (m/s).</param>
    /// <param name="acceleration">The constant acceleration vector (m/s²).</param>
    /// <param name="deltaTime">The time interval in seconds to project forward.</param>
    /// <returns>
    /// The predicted position using <c>p + v*t + 0.5*a*t²</c>.
    /// </returns>
    /// <remarks>
    /// This is a pure prediction with no side effects (velocity is not modified). Useful for
    /// lead-target calculations and look-ahead collision avoidance.
    /// </remarks>
    public static Vec3 CalculateFuturePosition(Vec3 currentPosition, Vec3 velocity, Vec3 acceleration,
        double deltaTime)
    {
        return new Vec3(
            currentPosition.X + velocity.X * deltaTime + 0.5 * acceleration.X * deltaTime * deltaTime,
            currentPosition.Y + velocity.Y * deltaTime + 0.5 * acceleration.Y * deltaTime * deltaTime,
            currentPosition.Z + velocity.Z * deltaTime + 0.5 * acceleration.Z * deltaTime * deltaTime
        );
    }

    /// <summary>
    /// Estimates the time for two moving entities to reach a specified separation distance,
    /// assuming both maintain constant velocities.
    /// </summary>
    /// <param name="position1">World-space position of entity 1.</param>
    /// <param name="velocity1">Velocity vector of entity 1 (m/s).</param>
    /// <param name="position2">World-space position of entity 2.</param>
    /// <param name="velocity2">Velocity vector of entity 2 (m/s).</param>
    /// <param name="targetDistance">The desired separation distance in metres.</param>
    /// <returns>
    /// The estimated time in seconds to reach <paramref name="targetDistance"/> separation.
    /// Returns <c>0</c> if the entities are already at the target distance (within 0.01 m tolerance)
    /// and have negligible relative speed. Returns <see cref="double.PositiveInfinity"/> if:
    /// <list type="bullet">
    ///   <item>The relative speed along the line of sight is effectively zero (<c>&lt; 1e-6</c>).</item>
    ///   <item>The computed time is negative (entities are moving apart rather than converging).</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The calculation projects the relative velocity onto the line connecting the two entities
    /// to determine the closing speed. This is a linear approximation and does not account for
    /// acceleration or curved trajectories.
    /// </remarks>
    public static double CalculateTimeToReachDistance(
        Vec3 position1, Vec3 velocity1, Vec3 position2, Vec3 velocity2, double targetDistance)
    {
        var relativePosition = position2 - position1;
        var relativeVelocity = velocity2 - velocity1;

        var currentDistance = relativePosition.Size();
        var relativeSpeed = relativePosition.Dot(relativeVelocity) / currentDistance;

        if (System.Math.Abs(relativeSpeed) < 1e-6)
        {
            return System.Math.Abs(currentDistance - targetDistance) < 0.01 ? 0 : double.PositiveInfinity;
        }

        var time = (currentDistance - targetDistance) / relativeSpeed;
        return time >= 0 ? time : double.PositiveInfinity;
    }

    /// <summary>
    /// Back-calculates the constant acceleration vector required to move from
    /// <paramref name="initialPosition"/> to <paramref name="finalPosition"/> in
    /// <paramref name="deltaTime"/> seconds, given an initial velocity.
    /// </summary>
    /// <param name="initialPosition">The starting position.</param>
    /// <param name="finalPosition">The desired ending position.</param>
    /// <param name="initialVelocity">The velocity at the start of the interval (m/s).</param>
    /// <param name="deltaTime">The time interval in seconds. Must be positive.</param>
    /// <returns>
    /// The required constant acceleration vector (m/s²), derived from the kinematic equation
    /// <c>a = 2 * (d - v₀*t) / t²</c> where <c>d = finalPosition - initialPosition</c>.
    /// Returns <see cref="Vec3.Zero"/> if <paramref name="deltaTime"/> is zero or negative.
    /// </returns>
    /// <remarks>
    /// Useful for computing the thrust an NPC needs to apply in order to arrive at a specific
    /// position after a known time interval, given its current velocity.
    /// </remarks>
    public static Vec3 CalculateAcceleration(Vec3 initialPosition, Vec3 finalPosition,
        Vec3 initialVelocity, double deltaTime)
    {
        if (deltaTime <= 0) return Vec3.Zero;

        var displacement = finalPosition - initialPosition;

        return new Vec3(
            2 * (displacement.X - initialVelocity.X * deltaTime) / (deltaTime * deltaTime),
            2 * (displacement.Y - initialVelocity.Y * deltaTime) / (deltaTime * deltaTime),
            2 * (displacement.Z - initialVelocity.Z * deltaTime) / (deltaTime * deltaTime)
        );
    }
}
