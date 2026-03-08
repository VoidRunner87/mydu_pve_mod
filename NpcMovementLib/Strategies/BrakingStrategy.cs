using NpcMovementLib.Math;

namespace NpcMovementLib.Strategies;

public class BrakingStrategy : IMovementStrategy
{
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
