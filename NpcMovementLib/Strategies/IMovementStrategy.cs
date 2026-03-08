using NpcMovementLib.Math;

namespace NpcMovementLib.Strategies;

public interface IMovementStrategy
{
    MoveResult Move(MoveParams @params);

    public class MoveParams
    {
        public required Vec3 Position { get; init; }
        public required Vec3 TargetPosition { get; init; }
        public required Vec3 Velocity { get; init; }
        public required Vec3 Acceleration { get; init; }
        public required double MaxVelocity { get; init; }
        public required double MaxVelocityGoal { get; init; }
        public required double MaxAcceleration { get; init; }
        public required double DeltaTime { get; init; }
        public double EnginePower { get; init; } = 1;
        public Vec3? PreviousVelocity { get; init; }
    }

    public class MoveResult
    {
        public required Vec3 Position { get; init; }
        public required Vec3 Velocity { get; init; }
    }
}
