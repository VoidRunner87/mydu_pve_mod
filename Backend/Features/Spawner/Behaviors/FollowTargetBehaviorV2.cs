using System;
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

public class FollowTargetBehaviorV2(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private TimePoint _timePoint = new();

    private bool _active = true;
    private IConstructService _constructService;
    private ILogger<FollowTargetBehaviorV2> _logger;

    public bool IsActive() => _active;

    public BehaviorTaskCategory Category => BehaviorTaskCategory.MovementPriority;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        
        _logger = provider.CreateLogger<FollowTargetBehaviorV2>();
        _constructService = provider.GetRequiredService<IConstructService>();

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
        
        // first time initialize position
        if (!context.Position.HasValue)
        {
            context.Position = npcConstructInfo.rData.position;
        }

        var targetPos = targetConstructInfo.rData.position;
        var npcPos = context.Position.Value;
        var targetDirection = targetPos - npcPos;

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
        
        // context.Velocity = velocity;
        // Make the ship point to where it's accelerating
        var accelerationFuturePos = npcPos + moveDirection * 200000;

        var rotation = VectorMathUtils.SetRotationToMatchDirection(
            npcPos.ToVector3(),
            accelerationFuturePos.ToVector3()
        );

        GetD0(context, out var d0, new Vec3());
        var relativeAngularVel = VectorMathHelper.CalculateAngularVelocity(
            d0,
            targetDirection,
            context.DeltaTime
        );
        
        SetD0(moveDirection, context);

        context.Rotation = rotation.ToNqQuat();

        _timePoint = TimePoint.Now();

        context.Velocity = (position - npcPos) / context.DeltaTime; 
        
        try
        {
            context.Position = position;
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

    
}