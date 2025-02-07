using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Vector.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;

public class CalculateTargetMovePositionWithOffsetEffect(IServiceProvider provider) : ICalculateTargetMovePositionEffect
{
    private readonly Random _random = provider.GetRandomProvider().GetRandom();
    
    private DateTime? LastTimeOffsetUpdated { get; set; }
    private Vec3 Offset { get; set; }
    
    public async Task<TargetMovePositionCalculationOutcome> GetTargetMovePosition(
        ICalculateTargetMovePositionEffect.Params @params)
    {
        if (!@params.TargetConstructId.HasValue ||
            !@params.InstigatorPosition.HasValue ||
            !@params.InstigatorStartPosition.HasValue)
        {
            return TargetMovePositionCalculationOutcome.Invalid();
        }

        var constructService = provider.GetRequiredService<IConstructService>();
        var logger = provider.CreateLogger<CalculateTargetMovePositionWithOffsetEffect>();

        var targetConstructTransformOutcome =
            await constructService.GetConstructTransformAsync(@params.TargetConstructId.Value);
        if (!targetConstructTransformOutcome.ConstructExists)
        {
            logger.LogError(
                "Construct {Construct} Target construct info {Target} is null",
                @params.InstigatorConstructId,
                @params.TargetConstructId.Value
            );

            return TargetMovePositionCalculationOutcome.MoveToAlternatePosition(@params.InstigatorStartPosition.Value);
        }

        var targetPos = targetConstructTransformOutcome.Position;

        var distanceFromTarget = (targetPos - @params.InstigatorPosition.Value).Size();
        if (distanceFromTarget > @params.MaxDistanceVisibility)
        {
            return TargetMovePositionCalculationOutcome.MoveToAlternatePosition(@params.InstigatorStartPosition.Value);
        }

        var distanceGoal = @params.TargetMoveDistance / 2;

        var timeDiff = DateTime.UtcNow - (LastTimeOffsetUpdated ?? DateTime.UtcNow);
        if (LastTimeOffsetUpdated == null || timeDiff > TimeSpan.FromSeconds(30))
        {
            Offset = _random.RandomDirectionVec3() * distanceGoal;
            LastTimeOffsetUpdated = DateTime.UtcNow;
        }

        // var futurePosition = VelocityHelper.CalculateFuturePosition(
        //     targetPos,
        //     @params.TargetConstructLinearVelocity,
        //     @params.TargetConstructAcceleration,
        //     @params.PredictionSeconds
        // );

        // logger.LogInformation("FUTURE POS DELTA: {A} {D}", @params.TargetConstructAcceleration,
        //     futurePosition - targetPos);
        //targetPos + Offset
        return TargetMovePositionCalculationOutcome.ValidCalculation(targetPos + Offset);
    }
}