using System;
using System.Linq;
using System.Threading.Tasks;
using BotLib.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Vector;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQutils.Exceptions;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class WaypointMoveBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private IConstructService _constructService;
    private ILogger<WaypointMoveBehavior> _logger;
    private IConstructHandleRepository _constructHandleService;

    public BehaviorTaskCategory Category => BehaviorTaskCategory.HighPriority;

    public async Task InitializeAsync(BehaviorContext context)
    {
        await Task.Yield();

        var provider = context.ServiceProvider;

        _constructService = provider.GetRequiredService<IConstructService>();
        _constructHandleService = provider.GetRequiredService<IConstructHandleRepository>();
        _logger = provider.CreateLogger<WaypointMoveBehavior>();
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            return;
        }

        if (context.TargetWaypoint == null)
        {
            context.TargetWaypoint = context.Waypoints.FirstOrDefault();
            if (context.TargetWaypoint != null)
            {
                context.TargetMovePosition = context.TargetWaypoint.Position;
            }
            else
            {
                await Despawn();
                return;
            }
        }
        
        var npcConstructInfo = await _constructService.GetConstructInfoAsync(constructId);
        if (npcConstructInfo == null)
        {
            return;
        }
        var npcPos = npcConstructInfo.rData.position;

        // Arrived Near Destination
        if (context.TargetMovePosition.Dist(npcPos) <= 50000)
        {
            context.TargetWaypoint.Visited = true;
            context.TargetWaypoint = context.Waypoints.FirstOrDefault(w => !w.Visited);
            if (context.TargetWaypoint != null)
            {
                context.TargetMovePosition = context.TargetWaypoint.Position;
            }
            else
            {
                // No more waypoints. Arrived. Despawn
                await Despawn();
                return;
            }
        }
        
        var moveDirection = (context.TargetMovePosition - npcPos).NormalizeSafe();

        var velocityDirection = context.Velocity.NormalizeSafe();
        var velToTargetDot = velocityDirection.Dot(moveDirection);

        double acceleration = prefab.DefinitionItem.AccelerationG * 9.81f;

        if (velToTargetDot < 0)
        {
            acceleration *= 1 + Math.Abs(velToTargetDot);
        }

        var accelV = moveDirection * acceleration;

        context.Velocity += accelV * context.DeltaTime;
        context.Velocity = context.Velocity.ClampToSize(prefab.DefinitionItem.MaxSpeedKph / 3.6d);
        var velocity = context.Velocity;

        var position = VelocityHelper.LinearInterpolateWithVelocity(
            npcPos,
            context.TargetMovePosition,
            ref velocity,
            accelV,
            context.DeltaTime
        );

        context.Velocity = velocity;

        // Make the ship point to where it's accelerating
        var accelerationFuturePos = npcPos + moveDirection * 200000;

        var rotation = VectorMathUtils.SetRotationToMatchDirection(
            npcPos.ToVector3(),
            accelerationFuturePos.ToVector3()
        );

        context.Rotation = rotation.ToNqQuat();

        var timePoint = TimePoint.Now();

        try
        {
            var cUpdate = new ConstructUpdate
            {
                pilotId = ModBase.Bot.PlayerId,
                constructId = constructId,
                rotation = context.Rotation,
                position = position,
                worldAbsoluteVelocity = context.Velocity,
                worldRelativeVelocity = context.Velocity,
                // worldAbsoluteAngVelocity = relativeAngularVel,
                // worldRelativeAngVelocity = relativeAngularVel,
                time = timePoint,
                grounded = false,
            };

            await ModBase.Bot.Req.ConstructUpdate(cUpdate);
        }
        catch (BusinessException be)
        {
            _logger.LogError(be, "Failed to update construct transform. Attempting a restart of the bot connection.");

            try
            {
                await ModBase.Bot.Reconnect();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to Reconnect");
            }
        }
    }

    private async Task Despawn()
    {
        await _constructHandleService.RemoveHandleAsync(constructId);
        await _constructService.SoftDeleteAsync(constructId);
        _logger.LogInformation("Despawned Construct {Construct}", constructId);
    }
}