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

public class FollowTargetBehaviorV2(ulong constructId, IConstructDefinition constructDefinition) : IConstructBehavior
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

        var distanceGoal = constructDefinition.DefinitionItem.TargetDistance;
        var offset = new Vec3 { y = distanceGoal };
        var targetFiringPos = targetPos + offset;

        var distance = targetPos.Distance(npcPos);
        
        var direction = (targetPos - npcPos + offset).NormalizeSafe();
        var pointDirection = (targetPos - npcPos).NormalizeSafe();
        
        var velocityDirection = context.Velocity.NormalizeSafe();
        var velToTargetDot = velocityDirection.Dot(direction);

        double acceleration = constructDefinition.DefinitionItem.AccelerationG * 9.81f;

        if (velToTargetDot < 0)
        {
            acceleration *= 1 + Math.Abs(velToTargetDot);
        }
        
        var accelV = direction * acceleration;

        if (distance <= distanceGoal * 2)
        {
            accelV = new Vec3();
        }

        context.Velocity += accelV * context.DeltaTime;
        context.Velocity = context.Velocity.ClampToSize(constructDefinition.DefinitionItem.MaxSpeedKph / 3.6d);
        var velocity = context.Velocity;

        var position = LerpWithVelocity(
            npcPos,
            targetFiringPos,
            ref velocity,
            accelV,
            context.DeltaTime
        );

        context.Velocity = velocity;
        
        var rotation = VectorMathHelper.CalculateRotationToPoint(
            npcPos,
            targetPos
        );

        _timePoint = TimePoint.Now();

        try
        {
            await ModBase.Bot.Req.ConstructUpdate(
                new ConstructUpdate
                {
                    constructId = constructId,
                    rotation = rotation,
                    position = position,
                    worldAbsoluteVelocity = context.Velocity,
                    worldAbsoluteAngVelocity = new Vec3(),
                    worldRelativeAngVelocity = new Vec3(),
                    worldRelativeVelocity = context.Velocity,
                    time = _timePoint,
                    grounded = false,
                }
            );
        }
        catch (BusinessException be)
        {
            _logger.LogError(be, "Failed to update construct transform. Attempting a restart of the bot connection.");

            await ModBase.Bot.Reconnect();
        }
    }

    public static Vec3 LerpWithVelocity(Vec3 start, Vec3 end, ref Vec3 velocity, Vec3 acceleration, double deltaTime)
    {
        // Calculate direction and distance to the end
        Vec3 direction = new Vec3
        {
            x = end.x - start.x,
            y = end.y - start.y,
            z = end.z - start.z
        };

        double distance = direction.Size();

        // Check if distance is very small (to avoid division by zero)
        if (distance < 0.001)
        {
            // Close to the destination; set position to the end and stop
            return new Vec3 { x = end.x, y = end.y, z = end.z };
        }

        // Update velocity based on acceleration
        velocity = new Vec3
        {
            x = velocity.x + acceleration.x * deltaTime,
            y = velocity.y + acceleration.y * deltaTime,
            z = velocity.z + acceleration.z * deltaTime
        };

        // Calculate the new position based on the updated velocity
        Vec3 newPosition = new Vec3
        {
            x = start.x + velocity.x * deltaTime,
            y = start.y + velocity.y * deltaTime,
            z = start.z + velocity.z * deltaTime
        };

        // Calculate the new distance to the end
        Vec3 newDirection = new Vec3
        {
            x = end.x - newPosition.x,
            y = end.y - newPosition.y,
            z = end.z - newPosition.z
        };

        double newDistance = newDirection.Size();

        // Check if the object is close to the target and the distance change is smaller than acceleration
        if (newDistance < 0.001 || newDistance < acceleration.Size() * deltaTime)
        {
            // Close to the destination; set position to the end and stop velocity
            newPosition = new Vec3 { x = end.x, y = end.y, z = end.z };
            velocity = new Vec3 { x = 0, y = 0, z = 0 };
        }

        // Check for NaN values and handle them
        if (double.IsNaN(newPosition.x) || double.IsNaN(newPosition.y) || double.IsNaN(newPosition.z) ||
            double.IsNaN(velocity.x) || double.IsNaN(velocity.y) || double.IsNaN(velocity.z))
        {
            // Handle NaN case by setting position to end and stopping velocity
            newPosition = new Vec3 { x = end.x, y = end.y, z = end.z };
            velocity = new Vec3 { x = 0, y = 0, z = 0 };
        }

        return newPosition;
    }
}