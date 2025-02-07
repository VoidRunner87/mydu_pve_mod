using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;

public interface IMovementEffect : IEffect
{
    Outcome Move(Params @params, BehaviorContext context);

    public class Params
    {
        public required Vec3 Position { get; init; }
        public required Vec3 TargetPosition { get; init; }
        public required Vec3 Velocity { get; init; }
        public required Vec3 Acceleration { get; init; }
        public required double MaxVelocity { get; init; }
        public required double MaxVelocityGoal { get; init; }
        public required double MaxAcceleration { get; init; }
        public required double DeltaTime { get; init; }
    }
    
    public class Outcome : IOutcome
    {
        public required Vec3 Position { get; init; }
        public required Vec3 Velocity { get; init; }
    }
}