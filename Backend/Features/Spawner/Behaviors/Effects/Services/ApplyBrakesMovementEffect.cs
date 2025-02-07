using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Vector.Helpers;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;

public class ApplyBrakesMovementEffect : IMovementEffect
{
    public IMovementEffect.Outcome Move(IMovementEffect.Params @params, BehaviorContext context)
    {
        var velocity = @params.Velocity;
        var acceleration = @params.Acceleration;
        
        var position = VelocityHelper.ApplyBraking(
            @params.Position,
            ref velocity,   
            acceleration.Size(),
            context.DeltaTime
        );

        return new IMovementEffect.Outcome
        {
            Position = position,
            Velocity = velocity
        };
    }
}