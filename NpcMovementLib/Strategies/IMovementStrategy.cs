using NpcCommonLib.Math;

namespace NpcMovementLib.Strategies;

/// <summary>
/// Defines a movement strategy that computes updated position and velocity for an NPC
/// construct given its current kinematic state and a target destination.
/// </summary>
/// <remarks>
/// The game's <c>FollowTargetBehaviorV2</c> behavior selects an active <c>IMovementEffect</c>
/// (the Backend equivalent of this interface) each tick. By default, the
/// <see cref="BurnToTargetStrategy"/> is used. When the NPC is braking or its engine power
/// is zero, the system temporarily switches to <see cref="BrakingStrategy"/>.
/// <see cref="PIDMovementStrategy"/> provides an alternative closed-loop approach.
/// <para>
/// Implementations are expected to be stateless per call; any inter-frame state
/// (such as the previous velocity for delta-V clamping) is passed in through
/// <see cref="MoveParams"/>.
/// </para>
/// </remarks>
public interface IMovementStrategy
{
    /// <summary>
    /// Computes the next position and velocity of an NPC construct for a single simulation tick.
    /// </summary>
    /// <param name="params">
    /// The complete kinematic state and movement constraints for this tick.
    /// </param>
    /// <returns>
    /// A <see cref="MoveResult"/> containing the updated world-space position and velocity
    /// vectors to apply to the construct.
    /// </returns>
    MoveResult Move(MoveParams @params);

    /// <summary>
    /// Contains all input parameters needed by a movement strategy to compute a single tick.
    /// </summary>
    /// <remarks>
    /// Mirrors the Backend's <c>IMovementEffect.Params</c> combined with relevant
    /// <c>BehaviorContext</c> properties. All spatial values (position, velocity, acceleration)
    /// use the game's world-space coordinate system where distances are in metres, velocities
    /// in m/s, acceleration in m/s², and time is in seconds.
    /// </remarks>
    public class MoveParams
    {
        /// <summary>
        /// The current world-space position of the NPC construct, in metres.
        /// </summary>
        public required Vec3 Position { get; init; }

        /// <summary>
        /// The world-space position the NPC is trying to reach, in metres.
        /// This is typically the player target's position or the next waypoint.
        /// </summary>
        public required Vec3 TargetPosition { get; init; }

        /// <summary>
        /// The current velocity of the NPC construct, in m/s.
        /// </summary>
        public required Vec3 Velocity { get; init; }

        /// <summary>
        /// The directional acceleration vector available to the NPC, in m/s².
        /// In <see cref="BurnToTargetStrategy"/>, this is scaled by <see cref="EnginePower"/>
        /// before use. Its magnitude is derived from the construct's configured <c>AccelerationG</c>
        /// multiplied by 9.81 and blended between the construct's forward direction and the
        /// move direction based on the realism factor.
        /// </summary>
        public required Vec3 Acceleration { get; init; }

        /// <summary>
        /// The absolute maximum speed the NPC is allowed to reach, in m/s.
        /// Velocity is hard-clamped to this value after each tick.
        /// </summary>
        public required double MaxVelocity { get; init; }

        /// <summary>
        /// The desired cruise speed for the current leg of travel, in m/s.
        /// Used by <see cref="BurnToTargetStrategy"/> via
        /// <see cref="VelocityHelper.LinearInterpolateWithAccelerationV2"/> to smoothly
        /// accelerate toward or decelerate from this goal speed depending on whether
        /// the current speed is above or below it.
        /// </summary>
        public required double MaxVelocityGoal { get; init; }

        /// <summary>
        /// The scalar maximum acceleration magnitude, in m/s².
        /// Used by <see cref="PIDMovementStrategy"/> to clamp the PID output and to determine
        /// the braking deceleration rate.
        /// </summary>
        public required double MaxAcceleration { get; init; }

        /// <summary>
        /// The simulation time step for this tick, in seconds.
        /// Typically driven by the server's behavior tick rate.
        /// </summary>
        public required double DeltaTime { get; init; }

        /// <summary>
        /// A multiplier (0.0 to 1.0) representing the NPC's current engine power output.
        /// A value of <c>0</c> typically triggers a switch to <see cref="BrakingStrategy"/>.
        /// Defaults to <c>1</c> (full power).
        /// </summary>
        public double EnginePower { get; init; } = 1;

        /// <summary>
        /// The velocity from the previous tick, used by <see cref="BurnToTargetStrategy"/>
        /// to clamp the change in velocity (delta-V) per tick so the NPC does not exceed
        /// its physical acceleration limits. If <c>null</c>, the current velocity is used
        /// as the baseline (i.e., no delta-V clamping on the first tick).
        /// </summary>
        public Vec3? PreviousVelocity { get; init; }
    }

    /// <summary>
    /// The output of a single movement tick, containing the updated position and velocity
    /// to apply to the NPC construct.
    /// </summary>
    public class MoveResult
    {
        /// <summary>
        /// The new world-space position of the NPC construct after this tick, in metres.
        /// </summary>
        public required Vec3 Position { get; init; }

        /// <summary>
        /// The new velocity of the NPC construct after this tick, in m/s.
        /// This value should be passed back as <see cref="MoveParams.Velocity"/> on the next tick.
        /// </summary>
        public required Vec3 Velocity { get; init; }
    }
}
