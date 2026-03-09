using NpcCommonLib.Math;
using NpcMovementLib.Math;

namespace NpcMovementLib.Strategies;

/// <summary>
/// Decelerates the NPC to a full stop by applying reverse thrust on each axis independently.
/// Selected automatically when the NPC's engine power is zero or it is flagged as braking.
/// </summary>
/// <remarks>
/// Ported from the Backend's <c>ApplyBrakesMovementEffect</c>. In the game's
/// <c>FollowTargetBehaviorV2</c>, this strategy is temporarily activated (for 1 second)
/// whenever <c>enginePower &lt;= 0</c> or <c>context.IsBraking()</c> returns <c>true</c>,
/// overriding the default <see cref="BurnToTargetStrategy"/>.
/// <para>
/// <b>Algorithm:</b> Delegates to <see cref="VelocityHelper.ApplyBraking"/>, which
/// reduces each velocity component (X, Y, Z) independently toward zero by
/// <c>decelerationRate * deltaTime</c> per tick. If a component's absolute value is
/// already less than or equal to the per-tick deceleration amount, it snaps to zero
/// to prevent oscillation around the stop point. Position is then integrated using
/// the reduced velocity.
/// </para>
/// <para>
/// The deceleration rate is the magnitude of the <see cref="IMovementStrategy.MoveParams.Acceleration"/>
/// vector, meaning the NPC brakes at the same rate it would accelerate.
/// </para>
/// </remarks>
public class BrakingStrategy : IMovementStrategy
{
    /// <summary>
    /// Decelerates the NPC toward a full stop and computes the resulting position.
    /// </summary>
    /// <param name="params">
    /// The kinematic state for this tick. Key fields:
    /// <see cref="IMovementStrategy.MoveParams.Velocity"/> (current velocity to reduce),
    /// <see cref="IMovementStrategy.MoveParams.Acceleration"/> (its magnitude is used as the
    /// deceleration rate, in m/s²),
    /// <see cref="IMovementStrategy.MoveParams.DeltaTime"/> (tick duration in seconds).
    /// The <see cref="IMovementStrategy.MoveParams.TargetPosition"/>,
    /// <see cref="IMovementStrategy.MoveParams.MaxVelocity"/>,
    /// <see cref="IMovementStrategy.MoveParams.MaxVelocityGoal"/>, and
    /// <see cref="IMovementStrategy.MoveParams.EnginePower"/> fields are not used by this strategy.
    /// </param>
    /// <returns>
    /// A <see cref="IMovementStrategy.MoveResult"/> with the updated position (drifting at
    /// reduced speed) and the new velocity vector (closer to zero on each axis).
    /// When the NPC has fully stopped, the returned velocity will be <see cref="Vec3.Zero"/>.
    /// </returns>
    public IMovementStrategy.MoveResult Move(IMovementStrategy.MoveParams @params)
    {
        var velocity = @params.Velocity;
        var acceleration = @params.Acceleration;

        var position = VelocityHelper.ApplyBraking(
            @params.Position,
            ref velocity,
            acceleration.Size(),
            @params.DeltaTime
        );

        return new IMovementStrategy.MoveResult
        {
            Position = position,
            Velocity = velocity
        };
    }
}
