using NpcMovementLib.Math;

namespace NpcMovementLib.Strategies;

public class PIDMovementStrategy : IMovementStrategy
{
    public double Kp { get; set; } = 0.2d;
    public double Kd { get; set; } = 0.3d;
    public double Ki { get; set; } = 0d;

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
