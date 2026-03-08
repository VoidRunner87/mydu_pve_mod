using NpcMovementLib.Math;

namespace NpcMovementLib.Strategies;

public class BurnToTargetStrategy : IMovementStrategy
{
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
