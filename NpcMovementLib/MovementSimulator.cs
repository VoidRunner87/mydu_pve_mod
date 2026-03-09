using System.Numerics;
using NpcMovementLib.Data;
using NpcCommonLib.Math;
using NpcMovementLib.Math;
using NpcMovementLib.Strategies;

namespace NpcMovementLib;

/// <summary>
/// The main entry point for NPC movement simulation. Orchestrates velocity goal calculation,
/// strategy selection, acceleration, and rotation into a single <see cref="Tick"/> call.
/// </summary>
/// <remarks>
/// <para>
/// This class is the library-side equivalent of
/// <c>FollowTargetBehaviorV2.TickAsync</c> in the game backend. The backend version is
/// tightly coupled to Orleans grains, <c>BehaviorContext</c>, and the <c>IMovementEffect</c>
/// effect system. <see cref="MovementSimulator"/> extracts the same physics logic into a
/// pure, dependency-free form so it can be unit-tested and reused outside the game server.
/// </para>
/// <para>
/// Each call to <see cref="Tick"/> performs these steps in order:
/// <list type="number">
///   <item>Derive the construct's forward vector from <see cref="MovementInput.Rotation"/>.</item>
///   <item>Compute the move direction toward <see cref="MovementInput.TargetMovePosition"/>.</item>
///   <item>Blend acceleration between the forward and move directions using
///         <see cref="MovementInput.RealismFactor"/> (0 = instant turn, 1 = realistic inertia).</item>
///   <item>Compute the velocity goal via <see cref="VelocityGoalCalculator"/> (accounts for
///         braking distance, weapon range, and speed modifiers).</item>
///   <item>Delegate position/velocity integration to an <see cref="IMovementStrategy"/>:
///         either <see cref="BrakingStrategy"/> (when engines are off or braking is active)
///         or <see cref="BurnToTargetStrategy"/> (default thrust-based movement).</item>
///   <item>Slerp the construct's rotation toward the movement direction at
///         <see cref="MovementInput.RotationSpeed"/>.</item>
/// </list>
/// </para>
/// <para>
/// <b>Thread safety:</b> This class holds no mutable state between calls.
/// A single instance can be shared across threads provided each call receives its own
/// <see cref="MovementInput"/>.
/// </para>
/// </remarks>
public class MovementSimulator
{
    private readonly IMovementStrategy _defaultStrategy;
    private readonly IMovementStrategy _brakingStrategy;

    /// <summary>
    /// Initializes a new instance of the <see cref="MovementSimulator"/> class.
    /// </summary>
    /// <param name="defaultStrategy">
    /// The movement strategy used when the NPC is actively thrusting (engines on, not braking).
    /// If <see langword="null"/>, defaults to <see cref="BurnToTargetStrategy"/>, which applies
    /// acceleration toward the target and clamps velocity to the computed goal.
    /// </param>
    public MovementSimulator(IMovementStrategy? defaultStrategy = null)
    {
        _defaultStrategy = defaultStrategy ?? new BurnToTargetStrategy();
        _brakingStrategy = new BrakingStrategy();
    }

    /// <summary>
    /// Executes a single simulation tick, advancing the NPC's position, velocity, and rotation.
    /// </summary>
    /// <param name="input">
    /// A snapshot of the NPC's current state and movement parameters. All positional values are in
    /// metres (world-space), velocities in m/s, acceleration in <em>g</em> (multiples of 9.81 m/s²),
    /// speeds in km/h, and <see cref="MovementInput.DeltaTime"/> in seconds (typically 0.05 s for
    /// the 20 FPS fixed-step loop).
    /// </param>
    /// <returns>
    /// A <see cref="MovementOutput"/> containing the updated world-space position, absolute velocity
    /// (m/s), and rotation quaternion. The caller is responsible for pushing these values to the
    /// game server via <see cref="Interfaces.IConstructUpdateService.SendConstructUpdate"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The acceleration blending works as follows:
    /// <list type="bullet">
    ///   <item><c>accelForward = forward * acceleration * RealismFactor</c> — thrust along the ship's nose.</item>
    ///   <item><c>accelMove = moveDirection * acceleration * (1 - RealismFactor)</c> — thrust directly toward target.</item>
    /// </list>
    /// When <see cref="MovementInput.RealismFactor"/> is 0, the NPC can instantly change direction
    /// (arcade-style). When it is 1, the NPC must turn before it can accelerate toward the target
    /// (realistic flight model).
    /// </para>
    /// <para>
    /// If the NPC's velocity is opposing its forward direction (<c>velToTargetDot &lt; 0</c>),
    /// acceleration is boosted proportionally to help the construct reverse more quickly.
    /// </para>
    /// </remarks>
    public MovementOutput Tick(MovementInput input)
    {
        var npcPos = input.Position;
        var targetMovePos = input.TargetMovePosition;

        // Calculate forward direction from current rotation
        var forward = VectorMathUtils.GetForward(input.Rotation);
        var forwardVec = Vec3.FromVector3(forward).NormalizeSafe();

        // Move direction toward target
        var moveDirection = (targetMovePos - npcPos).NormalizeSafe();

        // Check velocity alignment with forward
        var velocityDirection = input.Velocity.NormalizeSafe();
        var velToTargetDot = velocityDirection.Dot(forwardVec);

        // Compute acceleration
        var acceleration = input.AccelerationG * 9.81;

        if (velToTargetDot < 0)
        {
            acceleration *= 1 + System.Math.Abs(velToTargetDot);
        }

        var accelForward = forwardVec * acceleration * input.RealismFactor;
        var accelMove = moveDirection * acceleration * (1 - input.RealismFactor);
        var accelV = accelForward + accelMove;

        // Calculate velocity goal
        var brakingDistance = VelocityHelper.CalculateBrakingDistance(
            input.Velocity.Size(), input.GetAccelerationMps());

        var velocityGoal = VelocityGoalCalculator.Calculate(new VelocityGoalCalculator.VelocityGoalInput
        {
            Distance = input.TargetMoveDistance,
            TargetDistance = input.TargetDistance,
            TargetLinearVelocity = input.TargetLinearVelocity,
            NpcVelocity = input.Velocity,
            MinVelocity = input.MinVelocity,
            MaxVelocity = input.MaxVelocity,
            WeaponOptimalRange = input.WeaponOptimalRange,
            Modifiers = input.Modifiers,
            HasOverrideTargetMovePosition = input.HasOverrideTargetMovePosition,
            OverrideMovePositionDistance = input.OverrideMovePositionDistance,
            BrakingDistance = brakingDistance
        });

        // Select strategy: braking or default
        var strategy = (input.EnginePower <= 0 || input.IsBraking)
            ? _brakingStrategy
            : _defaultStrategy;

        var moveResult = strategy.Move(new IMovementStrategy.MoveParams
        {
            Acceleration = accelV,
            Velocity = input.Velocity,
            Position = npcPos,
            TargetPosition = targetMovePos,
            MaxVelocity = input.MaxVelocity,
            MaxVelocityGoal = velocityGoal,
            MaxAcceleration = acceleration,
            DeltaTime = input.DeltaTime,
            EnginePower = input.EnginePower,
            PreviousVelocity = input.PreviousVelocity
        });

        // Calculate rotation — point toward movement direction
        var accelerationFuturePos = npcPos + moveDirection * 200000;
        var targetRotation = VectorMathUtils.SetRotationToMatchDirection(
            npcPos.ToVector3(),
            accelerationFuturePos.ToVector3()
        );

        var rotation = Quaternion.Slerp(
            input.Rotation,
            targetRotation,
            (float)(input.RotationSpeed * input.DeltaTime)
        );

        return new MovementOutput
        {
            Position = moveResult.Position,
            Velocity = moveResult.Velocity,
            Rotation = rotation
        };
    }
}
