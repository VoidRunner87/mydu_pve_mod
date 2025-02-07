using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using BotLib.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
using NQ;
using NQ.Interfaces;
using NQutils.Exceptions;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class FollowTargetBehaviorV2(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private TimePoint _timePoint = new();

    private IConstructService _constructService;
    private ILogger<FollowTargetBehaviorV2> _logger;

    public BehaviorTaskCategory Category => BehaviorTaskCategory.MovementPriority;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.Provider;

        _logger = provider.CreateLogger<FollowTargetBehaviorV2>();
        _constructService = provider.GetRequiredService<IConstructService>();

        return Task.CompletedTask;
    }

    public async Task TickAsync(BehaviorContext context)
    {
        // first time initialize position
        if (!context.Position.HasValue)
        {
            var npcConstructTransformOutcome = await _constructService.GetConstructTransformAsync(constructId);
            if (!npcConstructTransformOutcome.ConstructExists)
            {
                return;
            }

            context.SetPosition(npcConstructTransformOutcome.Position);
        }

        var npcPos = context.Position!.Value;
        var targetMovePos = context.GetTargetMovePosition();

        var forward = VectorMathUtils.GetForward(context.Rotation.ToQuat())
            .ToNqVec3()
            .NormalizeSafe();
        var moveDirection = (targetMovePos - npcPos).NormalizeSafe();

        var velocityDirection = context.Velocity.NormalizeSafe();
        var velToTargetDot = velocityDirection.Dot(forward);

        context.TryGetProperty(BehaviorContext.EnginePowerProperty, out double enginePower, 1);

        var acceleration = prefab.DefinitionItem.AccelerationG * 9.81f;

        if (velToTargetDot < 0)
        {
            acceleration *= 1 + Math.Abs(velToTargetDot);
        }

        var accelForward = forward * acceleration * context.RealismFactor;
        var accelMove = moveDirection * acceleration * (1 - context.RealismFactor);

        var accelV = accelForward + accelMove;

        var velocity = context.Velocity;

        if (enginePower <= 0 || context.IsBraking())
        {
            context.Effects.Activate<IMovementEffect>(
                new ApplyBrakesMovementEffect(),
                TimeSpan.FromSeconds(1)
            );
        }

        var moveOutcome = context.Effects.GetOrNull<IMovementEffect>()
            .Move(new IMovementEffect.Params
                {
                    Acceleration = accelV,
                    Velocity = velocity,
                    Position = npcPos,
                    TargetPosition = targetMovePos,
                    MaxVelocity = context.MaxVelocity,
                    MaxVelocityGoal = context.CalculateVelocityGoal(context.TargetMoveDistance),
                    MaxAcceleration = acceleration,
                    DeltaTime = context.DeltaTime
                },
                context
            );

        velocity = moveOutcome.Velocity;
        var position = moveOutcome.Position;

        context.Velocity = velocity;

        // context.Velocity = velocity;
        // Make the ship point to where it's accelerating
        var accelerationFuturePos = npcPos + moveDirection * 200000 * context.TargetRotationPositionMultiplier;

        var currentRotation = context.Rotation;
        var targetRotation = VectorMathUtils.SetRotationToMatchDirection(
            npcPos.ToVector3(),
            accelerationFuturePos.ToVector3()
        );

        var rotation = Quaternion.Slerp(
            currentRotation.ToQuat(),
            targetRotation,
            (float)(prefab.DefinitionItem.RotationSpeed * context.DeltaTime)
        );

        context.Rotation = rotation.ToNqQuat();

        _timePoint = TimePoint.Now();

        var velocityDisplay = (position - npcPos) / context.DeltaTime;

        try
        {
            context.SetPosition(position);
            var cUpdate = new ConstructUpdate
            {
                pilotId = ModBase.Bot.PlayerId,
                constructId = constructId,
                rotation = context.Rotation,
                position = position,
                worldAbsoluteVelocity = velocityDisplay,
                worldRelativeVelocity = velocityDisplay,
                time = _timePoint,
                grounded = false,
            };

            var sw = new Stopwatch();
            sw.Start();

            try
            {
                await ModBase.Bot.Req.ConstructUpdate(cUpdate);
            }
            catch (BusinessException bex)
            {
                if (bex.error.code == ErrorCode.InvalidSession)
                {
                    ConstructBehaviorContextCache.RaiseBotDisconnected();
                    ModBase.ServiceProvider
                        .CreateLogger<FollowTargetBehaviorV2>()
                        .LogError(bex, "Need to reconnect the Bot");
                }
            }
            catch (Exception e)
            {
                ModBase.ServiceProvider.CreateLogger<FollowTargetBehaviorV2>()
                    .LogError(e, "Failed to send Construct Update Fire-And-Forget");
            }

            StatsRecorder.Record("ConstructUpdate", sw.ElapsedMilliseconds);
        }
        catch (BusinessException be)
        {
            _logger.LogError(be, "Failed to update construct transform. Attempting a restart of the bot connection.");
        }
    }
}