using System;
using System.Threading.Tasks;
using Backend.Scenegraph;
using BotLib.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Exceptions;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class FollowTargetBehaviorV2(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private TimePoint _timePoint = new();

    private bool _active = true;
    private IConstructService _constructService;
    private ILogger<FollowTargetBehavior> _logger;
    private IConstructGrain _constructGrain;
    private IScenegraphAPI _sceneGraph;

    public bool IsActive() => _active;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        var orleans = provider.GetOrleans();
        
        _logger = provider.CreateLogger<FollowTargetBehavior>();
        _constructService = provider.GetRequiredService<IConstructService>();

        _constructGrain = orleans.GetConstructGrain(constructId);
        _sceneGraph = provider.GetRequiredService<IScenegraphAPI>();
        
        return Task.CompletedTask;
    }

    private void SetD0(Vec3 value, BehaviorContext context)
    {
        if (!context.ExtraProperties.TryAdd("d0", value))
        {
            context.ExtraProperties["d0"] = value;
        }
    }

    private bool GetD0(BehaviorContext context, out Vec3 value, Vec3 defaultValue)
    {
        if (!context.ExtraProperties.TryGetValue("d0", out value))
        {
            value = defaultValue;
            return false;
        }

        return true;
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

        var distanceGoal = prefab.DefinitionItem.TargetDistance;
        var offset = new Vec3 { y = distanceGoal };
        var targetFiringPos = targetPos + offset;

        var distance = targetPos.Distance(npcPos);
        var targetPosWithOffset = targetPos + offset;

        var direction = (targetPosWithOffset - npcPos).NormalizeSafe();

        var velocityDirection = context.Velocity.NormalizeSafe();
        var velToTargetDot = velocityDirection.Dot(direction);

        double acceleration = prefab.DefinitionItem.AccelerationG * 9.81f;

        if (velToTargetDot < 0)
        {
            acceleration *= 1 + Math.Abs(velToTargetDot);
        }

        var accelV = direction * acceleration;

        context.Velocity += accelV * context.DeltaTime;
        context.Velocity = context.Velocity.ClampToSize(prefab.DefinitionItem.MaxSpeedKph / 3.6d);
        var velocity = context.Velocity;

        Vec3 position;
        if (distance > distanceGoal * 1.25)
        {
            position = LerpWithVelocity(
                npcPos,
                targetFiringPos,
                ref velocity,
                accelV,
                context.DeltaTime
            );
        }
        else
        {
            position = npcPos + velocity * context.DeltaTime;
        }

        context.Velocity = velocity;

        // Make the ship point to where it's accelerating
        var accelerationFuturePos = npcPos + direction * 200000;

        var rotation = VectorMathUtils.SetRotationToMatchDirection(
            npcPos.ToVector3(),
            accelerationFuturePos.ToVector3()
        );

        var targetDirection = targetPos - npcPos;
        GetD0(context, out var d0, new Vec3());
        var relativeAngularVel = VectorMathHelper.CalculateAngularVelocity(
            d0,
            targetDirection,
            context.DeltaTime
        );
        
        SetD0(direction, context);

        context.Rotation = rotation.ToNqQuat();

        _timePoint = TimePoint.Now();

        // var (v, av) = await _constructGrain.GetConstructVelocity();
        // var isBeingControlled = await _constructGrain.IsBeingControlled();
        // var lastUpdate = await _sceneGraph.GetLastConstructUpdate(constructId);

        // _logger.LogInformation("Velocities: {Controlled} {V} | {luv}", isBeingControlled, v, lastUpdate?.worldAbsoluteVelocity);
        //_logger.LogInformation("Velocities: {Controlled} {V} | {luv}", isBeingControlled, av, lastUpdate?.worldAbsoluteAngVelocity);
        
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
                time = _timePoint,
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

    public static Vec3 CalculateAngularVelocity(Quat q1, Quat q2, double deltaTime)
    {
        // Step 1: Compute relative rotation
        var deltaQ = QuatHelpers.Multiply(q2, QuatHelpers.Inverse(q1));

        // Step 2: Calculate the angle of rotation (theta)
        var theta = 2 * Math.Acos(deltaQ.w);

        // Step 3: Calculate the rotation axis
        var sinHalfTheta = Math.Sqrt(1 - deltaQ.w * deltaQ.w);
        var rotationAxis = new Vec3 { x = deltaQ.x, y = deltaQ.y, z = deltaQ.z };

        if (sinHalfTheta > 0.001)
        {
            rotationAxis = new Vec3
            {
                x = deltaQ.x / sinHalfTheta, 
                y = deltaQ.y / sinHalfTheta, 
                z = deltaQ.z / sinHalfTheta
            };
        }
        else
        {
            rotationAxis = new Vec3 { x = 1, y = 0, z = 0 }; // Arbitrary direction
        }

        // Step 4: Compute angular velocity
        return new Vec3
        {
            x = rotationAxis.x * theta / deltaTime, 
            y = rotationAxis.y * theta / deltaTime,
            z = rotationAxis.z * theta / deltaTime
        };
    }

    public static Vec3 LerpWithVelocity(Vec3 start, Vec3 end, ref Vec3 velocity, Vec3 acceleration, double deltaTime)
    {
        // Calculate direction and distance to the end
        var direction = new Vec3
        {
            x = end.x - start.x,
            y = end.y - start.y,
            z = end.z - start.z
        };

        var distance = direction.Size();

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
        var newPosition = new Vec3
        {
            x = start.x + velocity.x * deltaTime,
            y = start.y + velocity.y * deltaTime,
            z = start.z + velocity.z * deltaTime
        };

        // Calculate the new distance to the end
        var newDirection = new Vec3
        {
            x = end.x - newPosition.x,
            y = end.y - newPosition.y,
            z = end.z - newPosition.z
        };

        var newDistance = newDirection.Size();

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