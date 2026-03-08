using System.Numerics;
using NpcMovementLib.Data;
using NpcMovementLib.Math;
using NpcMovementLib.Strategies;

namespace NpcMovementLib;

/// <summary>
/// Main orchestrator: takes a MovementInput, runs the movement strategy + rotation, returns MovementOutput.
/// This is the primary entry point for consumers.
/// </summary>
public class MovementSimulator
{
    private readonly IMovementStrategy _defaultStrategy;
    private readonly IMovementStrategy _brakingStrategy;

    public MovementSimulator(IMovementStrategy? defaultStrategy = null)
    {
        _defaultStrategy = defaultStrategy ?? new BurnToTargetStrategy();
        _brakingStrategy = new BrakingStrategy();
    }

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
