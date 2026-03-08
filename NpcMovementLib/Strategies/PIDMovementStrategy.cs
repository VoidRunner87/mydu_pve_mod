using NpcCommonLib.Math;

namespace NpcMovementLib.Strategies;

/// <summary>
/// Moves the NPC toward its target using a PID (Proportional-Integral-Derivative) controller
/// to compute a desired acceleration vector, with an automatic braking phase when the NPC
/// is within a configurable distance threshold.
/// </summary>
/// <remarks>
/// Ported from the Backend's <c>PIDMovementEffect</c>. Unlike <see cref="BurnToTargetStrategy"/>
/// which applies direct kinematic interpolation along the target direction, this strategy
/// uses closed-loop feedback control to smoothly converge on the target position.
/// <para>
/// <b>Algorithm overview:</b>
/// <list type="number">
///   <item>
///     Instantiate a <see cref="PIDController"/> with the configured gains
///     (<see cref="Kp"/>, <see cref="Ki"/>, <see cref="Kd"/>).
///   </item>
///   <item>
///     Compute a desired acceleration from the position error using PID control,
///     then clamp it to <see cref="IMovementStrategy.MoveParams.MaxAcceleration"/>.
///   </item>
///   <item>
///     If the distance to the target is less than the braking threshold (100,000 metres = 100 km),
///     override the PID output with full reverse thrust to decelerate.
///   </item>
///   <item>
///     Integrate velocity and position using Euler integration, clamping velocity to
///     <see cref="IMovementStrategy.MoveParams.MaxVelocity"/>.
///   </item>
/// </list>
/// </para>
/// <para>
/// <b>When to use:</b> Choose this strategy when you need smooth, oscillation-damped
/// convergence on a target, such as station-keeping or formation flying. The PID gains
/// can be tuned at runtime via the <c>PIDConfigurationController</c> API endpoint.
/// For simple straight-line pursuit, <see cref="BurnToTargetStrategy"/> is more efficient.
/// </para>
/// <para>
/// <b>Note:</b> A new <see cref="PIDController"/> is created each tick, so integral and
/// derivative state does not accumulate across ticks. This matches the original Backend
/// implementation. For persistent PID state, the controller instance would need to be
/// stored externally.
/// </para>
/// </remarks>
public class PIDMovementStrategy : IMovementStrategy
{
    /// <summary>
    /// Proportional gain for the PID controller. Controls how aggressively the NPC
    /// accelerates in proportion to its distance from the target. Higher values produce
    /// faster response but may cause overshoot.
    /// </summary>
    /// <value>Defaults to <c>0.2</c>.</value>
    public double Kp { get; set; } = 0.2d;

    /// <summary>
    /// Derivative gain for the PID controller. Damps oscillations by applying a corrective
    /// force proportional to the rate of change of the position error. Higher values
    /// reduce overshoot but slow convergence.
    /// </summary>
    /// <value>Defaults to <c>0.3</c>.</value>
    public double Kd { get; set; } = 0.3d;

    /// <summary>
    /// Integral gain for the PID controller. Corrects for steady-state error by accumulating
    /// the position error over time. Typically left at zero to avoid integral windup in
    /// a space environment with no persistent external forces.
    /// </summary>
    /// <value>Defaults to <c>0</c> (disabled).</value>
    public double Ki { get; set; } = 0d;

    /// <summary>
    /// Computes the next position and velocity using PID-controlled acceleration toward
    /// the target, with automatic braking when within 100,000 metres (100 km).
    /// </summary>
    /// <param name="params">
    /// The kinematic state and constraints for this tick. Key fields:
    /// <see cref="IMovementStrategy.MoveParams.MaxAcceleration"/> (clamp for PID output and braking thrust),
    /// <see cref="IMovementStrategy.MoveParams.MaxVelocity"/> (hard speed cap).
    /// <see cref="IMovementStrategy.MoveParams.EnginePower"/> is not used by this strategy.
    /// </param>
    /// <returns>
    /// A <see cref="IMovementStrategy.MoveResult"/> with the Euler-integrated position and
    /// velocity after applying the PID-computed or braking acceleration.
    /// </returns>
    public IMovementStrategy.MoveResult Move(IMovementStrategy.MoveParams @params)
    {
        var deltaTime = @params.DeltaTime;
        var npcVelocity = @params.Velocity;
        var npcPosition = @params.Position;
        var playerPosition = @params.TargetPosition;
        var maxAcceleration = @params.MaxAcceleration;
        var maxSpeed = @params.MaxVelocity;
        var deadZone = 1.0;
        var brakingThreshold = 100000;

        var pid = new PIDController(Kp, Ki, Kd);

        Vec3 desiredAcceleration = pid.Compute(npcPosition, playerPosition, deltaTime, deadZone);

        desiredAcceleration = desiredAcceleration.ClampToSize(maxAcceleration);

        double distanceToTarget = (playerPosition - npcPosition).Size();
        if (distanceToTarget < brakingThreshold)
        {
            desiredAcceleration = npcVelocity.NormalizeSafe().Reverse() * maxAcceleration;
        }

        npcVelocity = new Vec3(
            npcVelocity.X + desiredAcceleration.X * deltaTime,
            npcVelocity.Y + desiredAcceleration.Y * deltaTime,
            npcVelocity.Z + desiredAcceleration.Z * deltaTime
        );

        npcVelocity = npcVelocity.ClampToSize(maxSpeed);

        npcPosition = new Vec3(
            npcPosition.X + npcVelocity.X * deltaTime,
            npcPosition.Y + npcVelocity.Y * deltaTime,
            npcPosition.Z + npcVelocity.Z * deltaTime
        );

        return new IMovementStrategy.MoveResult
        {
            Position = npcPosition,
            Velocity = npcVelocity
        };
    }
}
