using NpcCommonLib.Math;

namespace NpcMovementLib.Strategies;

/// <summary>
/// A three-dimensional PID (Proportional-Integral-Derivative) controller that computes
/// a desired acceleration vector to drive a position toward a target.
/// </summary>
/// <remarks>
/// Ported from the Backend's <c>PIDController</c> in
/// <c>Mod.DynamicEncounters.Features.Spawner.Behaviors.Services</c>.
/// <para>
/// The controller operates on 3D position error vectors, applying independent PID
/// calculations across all three axes simultaneously. The output is a desired acceleration
/// vector (in m/s²) that, when applied to the NPC's velocity,
/// steers it toward the target position.
/// </para>
/// <para>
/// <b>PID terms:</b>
/// <list type="bullet">
///   <item><b>Proportional (Kp):</b> Output proportional to the current position error.
///   Provides the primary steering force.</item>
///   <item><b>Integral (Ki):</b> Output proportional to accumulated error over time.
///   Corrects persistent offsets but risks windup if not managed.</item>
///   <item><b>Derivative (Kd):</b> Output proportional to the rate of change of error.
///   Damps oscillations and reduces overshoot.</item>
/// </list>
/// </para>
/// <para>
/// <b>State:</b> This controller is stateful -- it maintains integral accumulation and
/// previous error across calls to <see cref="Compute"/>. However, the current
/// <see cref="PIDMovementStrategy"/> creates a new instance each tick, so in practice
/// the state does not persist across ticks unless the caller retains the instance.
/// </para>
/// </remarks>
public class PIDController
{
    private readonly double _kp;
    private readonly double _ki;
    private readonly double _kd;

    private Vec3 _integral;
    private Vec3 _previousError;

    /// <summary>
    /// Initializes a new PID controller with the specified gain parameters.
    /// </summary>
    /// <param name="kp">
    /// Proportional gain. Multiplied by the position error vector to produce the
    /// proportional term. Typical value: <c>0.2</c>.
    /// </param>
    /// <param name="ki">
    /// Integral gain. Multiplied by the time-accumulated error to produce the
    /// integral term. Typical value: <c>0</c> (disabled to avoid windup).
    /// </param>
    /// <param name="kd">
    /// Derivative gain. Multiplied by the rate of change of error to produce the
    /// derivative term. Typical value: <c>0.3</c>.
    /// </param>
    public PIDController(double kp, double ki, double kd)
    {
        _kp = kp;
        _ki = ki;
        _kd = kd;

        _integral = Vec3.Zero;
        _previousError = Vec3.Zero;
    }

    /// <summary>
    /// Computes the desired acceleration vector to move from the current position toward the target.
    /// </summary>
    /// <param name="currentPosition">
    /// The NPC's current world-space position, in metres.
    /// </param>
    /// <param name="targetPosition">
    /// The desired world-space position to converge on, in metres.
    /// </param>
    /// <param name="deltaTime">
    /// The time elapsed since the last call, in seconds. Used to scale the integral
    /// accumulation and derivative calculation. Must be greater than zero.
    /// </param>
    /// <param name="deadZone">
    /// The minimum position error magnitude (in metres) below which the controller
    /// returns <see cref="Vec3.Zero"/> instead of computing an output. Prevents jitter
    /// when the NPC is effectively at the target. Typical value: <c>1.0</c>.
    /// </param>
    /// <returns>
    /// A 3D acceleration vector (in m/s²) representing the
    /// combined PID output. Returns <see cref="Vec3.Zero"/> if the position error
    /// magnitude is less than <paramref name="deadZone"/>. The caller is responsible
    /// for clamping this to the NPC's maximum acceleration.
    /// </returns>
    public Vec3 Compute(Vec3 currentPosition, Vec3 targetPosition, double deltaTime, double deadZone)
    {
        var error = targetPosition - currentPosition;

        if (error.Size() < deadZone)
        {
            return Vec3.Zero;
        }

        var proportional = error * _kp;

        _integral = _integral + error * deltaTime;
        var integralTerm = _integral * _ki;

        var derivative = (error - _previousError) / deltaTime * _kd;

        _previousError = error;

        return proportional + integralTerm + derivative;
    }
}
