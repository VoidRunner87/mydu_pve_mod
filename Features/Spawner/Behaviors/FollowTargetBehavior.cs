using System;
using System.Threading.Tasks;
using BotLib.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQutils.Exceptions;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class FollowTargetBehavior(ulong constructId, IConstructDefinition constructDefinition) : IConstructBehavior
{
    private TimePoint _timePoint = new();

    private bool _active = true;
    private IConstructService _constructService;
    private ILogger<FollowTargetBehavior> _logger;

    public bool IsActive() => _active;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;

        _logger = provider.CreateLogger<FollowTargetBehavior>();
        _constructService = context.ServiceProvider.GetRequiredService<IConstructService>();
        
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

        var client = context.Client;

        if (context.TargetConstructId is null or 0)
        {
            return;
        }

        var targetConstructInfo = await _constructService.GetConstructInfoAsync(context.TargetConstructId.Value);
        var npcConstructInfo = await _constructService.GetConstructInfoAsync(constructId);

        if (targetConstructInfo == null || npcConstructInfo == null)
        {
            return;
        }

        var targetPos = targetConstructInfo.rData.position;
        var npcPos = npcConstructInfo.rData.position;

        var distance = targetPos.Distance(npcPos);

        var direction = (targetPos - npcPos + new Vec3 { y = constructDefinition.DefinitionItem.TargetDistance })
            .Normalized();
        var velocity = direction * constructDefinition.DefinitionItem.AccelerationG * 9.81f;

        var rotation = VectorMathHelper.CalculateRotationToPoint(
            npcConstructInfo.rData.position,
            direction
        );

        // Add acceleration to get close.
        // Otherwise coast 
        if (distance > constructDefinition.DefinitionItem.TargetDistance)
        {
            context.Velocity += velocity;
        }

        context.Velocity = context.Velocity.ClampToSize(constructDefinition.DefinitionItem.MaxSpeedKph / 3.6d);

        var finalVelocity = context.Velocity * Math.Clamp(context.DeltaTime, 1/60f, 1/15f);

        _timePoint = TimePoint.Now();

        try
        {
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
        catch (BusinessException be)
        {
            _logger.LogError(be, "Failed to update construct transform. Attempting a restart of the bot connection.");
            
            ModBase.Bot = await ModBase.RefreshClient();
            client = ModBase.Bot;
        }
    }
}