using System;
using System.Threading.Tasks;
using BotLib.Generated;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class FollowTargetBehavior(ulong constructId, IConstructDefinition constructDefinition) : IConstructBehavior
{
    private TimePoint _timePoint = new();

    private bool _active = true;

    public bool IsActive() => _active;

    public Task InitializeAsync(BehaviorContext context)
    {
        return Task.CompletedTask;
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            _active = false;

            return;
        }

        if (!context.TargetConstructId.HasValue)
        {
            return;
        }

        var provider = context.ServiceProvider;
        var orleans = provider.GetOrleans();
        var client = context.Client;

        if (context.TargetConstructId is null or 0)
        {
            return;
        }

        var targetConstructInfoGrain = orleans.GetConstructInfoGrain(context.TargetConstructId.Value);
        var targetConstructInfo = await targetConstructInfoGrain.Get();

        var npcConstructInfoGrain = orleans.GetConstructInfoGrain(constructId);
        var npcConstructInfo = await npcConstructInfoGrain.Get();

        var direction = (targetConstructInfo.rData.position - npcConstructInfo.rData.position + new Vec3 { y = constructDefinition.DefinitionItem.TargetDistance })
            .Normalized();
        var velocity = direction * constructDefinition.DefinitionItem.AccelerationG * 9.81f;

        var rotation = VectorMathHelper.CalculateRotationToPoint(
            npcConstructInfo.rData.position,
            direction
        );

        context.Velocity += velocity;
        context.Velocity = context.Velocity.ClampToSize(constructDefinition.DefinitionItem.MaxSpeedKph / 3.6d);

        var finalVelocity = context.Velocity * Math.Clamp(context.DeltaTime, 1/60f, 1/15f);

        _timePoint = TimePoint.Now();

        await client.Req.ConstructUpdate(
            new ConstructUpdate
            {
                constructId = constructId,
                rotation = rotation,
                position = npcConstructInfo.rData.position + finalVelocity,
                worldAbsoluteVelocity = new Vec3(),
                worldAbsoluteAngVelocity = new Vec3(),
                worldRelativeAngVelocity = new Vec3(),
                worldRelativeVelocity = finalVelocity,
                time = _timePoint,
                grounded = false,
            }
        );
    }
}