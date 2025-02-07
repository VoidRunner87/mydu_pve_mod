using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Vector.Helpers;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;

public class BurnToTargetMovementEffect : IMovementEffect
{
    public IMovementEffect.Outcome Move(IMovementEffect.Params @params, BehaviorContext context)
    {
        var velocity = @params.Velocity;
        context.TryGetProperty(BehaviorContext.EnginePowerProperty, out double enginePower, 1);
        var acceleration = @params.Acceleration * enginePower;

        var position = VelocityHelper.LinearInterpolateWithAccelerationV2(
            @params.Position,
            @params.TargetPosition,
            ref velocity,
            acceleration,
            @params.MaxVelocity,
            @params.MaxVelocityGoal,
            context.DeltaTime,
            true
        );

        context.TryGetProperty(BehaviorContext.V0Property, out var v0, velocity);

        var deltaV = velocity - v0;
        var maxDeltaV = @params.Acceleration.Size() * context.DeltaTime;

        if (deltaV.Size() > maxDeltaV)
        {
            deltaV = deltaV.NormalizeSafe() * maxDeltaV;
            velocity = v0 + deltaV;
            position = @params.Position + velocity * context.DeltaTime;
        }

        context.SetProperty(BehaviorContext.V0Property, velocity);

        return new IMovementEffect.Outcome
        {
            Position = position,
            Velocity = velocity
        };
    }
}