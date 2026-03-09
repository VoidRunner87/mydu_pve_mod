using NpcCommonLib.Math;
using NpcMovementLib.Math;

namespace NpcMovementLib.Strategies;

/// <summary>
/// Accelerates the NPC along the direction toward its target, adjusting speed toward
/// a velocity goal while clamping per-tick delta-V to prevent physically impossible
/// acceleration spikes.
/// </summary>
/// <remarks>
/// This is the default movement strategy used by the game's <c>FollowTargetBehaviorV2</c>
/// (via <c>BurnToTargetMovementEffect</c>). It is registered as the default
/// <c>IMovementEffect</c> in the effect handler and remains active unless explicitly
/// overridden by <see cref="BrakingStrategy"/> or <see cref="PIDMovementStrategy"/>.
/// <para>
/// <b>Algorithm overview:</b>
/// <list type="number">
///   <item>
///     Scale the acceleration vector by <see cref="IMovementStrategy.MoveParams.EnginePower"/>.
///   </item>
///   <item>
///     Call <see cref="VelocityHelper.LinearInterpolateWithAccelerationV2"/> which:
///     (a) steers velocity toward the target direction,
///     (b) ramps speed up or down toward <see cref="IMovementStrategy.MoveParams.MaxVelocityGoal"/>,
///     (c) integrates position using kinematic equations with overshoot protection.
///   </item>
///   <item>
///     Clamp the change in velocity (delta-V) since the previous tick so it does not
///     exceed <c>|Acceleration| * DeltaTime</c>. If clamped, recompute position using
///     simple Euler integration with the corrected velocity. This prevents sudden velocity
///     jumps when the target direction changes sharply.
///   </item>
/// </list>
/// </para>
/// <para>
/// Choose this strategy for standard pursuit/intercept movement where the NPC should
/// fly toward a target at its configured acceleration, smoothly reaching a goal speed
/// that varies with distance. For closed-loop correction (e.g., formation flying),
/// prefer <see cref="PIDMovementStrategy"/>.
/// </para>
/// </remarks>
public class BurnToTargetStrategy : IMovementStrategy
{
    /// <summary>
    /// Computes the next position and velocity by burning toward the target position,
    /// respecting engine power and delta-V limits.
    /// </summary>
    /// <param name="params">
    /// The kinematic state and constraints for this tick. Key fields:
    /// <see cref="IMovementStrategy.MoveParams.Acceleration"/> (direction and magnitude of thrust),
    /// <see cref="IMovementStrategy.MoveParams.EnginePower"/> (throttle multiplier),
    /// <see cref="IMovementStrategy.MoveParams.MaxVelocityGoal"/> (desired cruise speed),
    /// and <see cref="IMovementStrategy.MoveParams.PreviousVelocity"/> (for delta-V clamping).
    /// </param>
    /// <returns>
    /// A <see cref="IMovementStrategy.MoveResult"/> with the updated position and velocity.
    /// When delta-V clamping activates, the position is recomputed via simple Euler integration
    /// rather than the kinematic formula.
    /// </returns>
    public IMovementStrategy.MoveResult Move(IMovementStrategy.MoveParams @params)
    {
        var velocity = @params.Velocity;
        var acceleration = @params.Acceleration * @params.EnginePower;

        var position = VelocityHelper.LinearInterpolateWithAccelerationV2(
            @params.Position,
            @params.TargetPosition,
            ref velocity,
            acceleration,
            @params.MaxVelocity,
            @params.MaxVelocityGoal,
            @params.DeltaTime,
            true
        );

        var v0 = @params.PreviousVelocity ?? velocity;
        var deltaV = velocity - v0;
        var maxDeltaV = @params.Acceleration.Size() * @params.DeltaTime;

        if (deltaV.Size() > maxDeltaV)
        {
            deltaV = deltaV.NormalizeSafe() * maxDeltaV;
            velocity = v0 + deltaV;
            position = @params.Position + velocity * @params.DeltaTime;
        }

        return new IMovementStrategy.MoveResult
        {
            Position = position,
            Velocity = velocity
        };
    }
}
