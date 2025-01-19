using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using Mod.DynamicEncounters.Features.VoxelService.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class SelectTargetBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private bool _active = true;
    private IClusterClient _orleans;
    private ILogger<SelectTargetBehavior> _logger;
    private IConstructGrain _constructGrain;
    private IConstructService _constructService;
    private ISectorPoolManager _sectorPoolManager;
    private IAreaScanService _areaScanService;
    private IConstructDamageService _constructDamageService;
    private IVoxelServiceClient _pveVoxelService;
    private ISafeZoneService _safeZoneService;

    public bool IsActive() => _active;

    public BehaviorTaskCategory Category => BehaviorTaskCategory.MediumPriority;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.Provider;

        _orleans = provider.GetOrleans();
        _logger = provider.CreateLogger<SelectTargetBehavior>();
        _constructGrain = _orleans.GetConstructGrain(constructId);
        _constructService = provider.GetRequiredService<IConstructService>();
        _constructDamageService = provider.GetRequiredService<IConstructDamageService>();
        _sectorPoolManager = provider.GetRequiredService<ISectorPoolManager>();
        _areaScanService = provider.GetRequiredService<IAreaScanService>();
        _pveVoxelService = provider.GetRequiredService<IVoxelServiceClient>();
        _safeZoneService = provider.GetRequiredService<ISafeZoneService>();

        return Task.CompletedTask;
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            _active = false;
            return;
        }

        var targetSpan = DateTime.UtcNow - context.TargetSelectedTime;
        if (targetSpan < TimeSpan.FromSeconds(1))
        {
            var sameTargetMoveOutcome = await CalculateTargetMovePosition(context);
            if (!sameTargetMoveOutcome.Valid) return;

            var targetConstructId = context.GetTargetConstructId();
            if (targetConstructId.HasValue)
            {
                var velocities = await _constructService.GetConstructVelocities(targetConstructId.Value);

                context.SetAutoTargetMovePosition(sameTargetMoveOutcome.TargetMovePosition);
                context.SetTargetLinearVelocity(velocities.Linear);
            }

            return;
        }

        var sw = new Stopwatch();
        sw.Start();

        _logger.LogInformation("Construct {Construct} Selecting a new Target", constructId);

        if (!context.Position.HasValue)
        {
            return;
        }

        IList<ScanContact> radarContacts = [];

        if (context.Position.HasValue)
        {
            var safeZones = await _safeZoneService.GetSafeZones();
            
            var spatialQuerySw = new StopWatch();
            spatialQuerySw.Start();

            radarContacts = (await _areaScanService.ScanForPlayerContacts(
                    constructId,
                    context.Position.Value,
                    DistanceHelpers.OneSuInMeters * 8
                ))
                .Where(c => !safeZones.Any(sz => sz.IsPointInside(c.Position)))
                .ToList();

            await Task.WhenAll(radarContacts
                .Select(x => _pveVoxelService.TriggerConstructCacheAsync(x.ConstructId)));

            StatsRecorder.Record("NPC_Radar", sw.ElapsedMilliseconds);
        }

        context.UpdateRadarContacts(radarContacts);

        if (!context.HasAnyRadarContact())
        {
            context.SetAutoTargetMovePosition(context.StartPosition ?? context.Sector);
            context.SetAutoTargetConstructId(null);
            return;
        }

        context.RefreshIdleSince();

        var selectTargetEffect = context.Effects.GetOrNull<ISelectRadarTargetEffect>();

        var selectedTarget = selectTargetEffect?.GetTarget(
            new ISelectRadarTargetEffect.Params
            {
                DecisionTimeSeconds = prefab.DefinitionItem.TargetDecisionTimeSeconds,
                Contacts = radarContacts,
                Context = context
            }
        );

        if (selectedTarget == null)
        {
            return;
        }

        var targetId = selectedTarget.ConstructId;

        context.SetAutoTargetConstructId(targetId);

        var targetDamage = await _constructDamageService.GetConstructDamage(targetId);
        context.SetTargetDamageData(targetId, targetDamage);

        var outcome = await CalculateTargetMovePosition(context);
        if (!outcome.Valid) return;

        context.SetAutoTargetMovePosition(outcome.TargetMovePosition);

        var targetVel = await _constructService.GetConstructVelocities(targetId);
        context.SetTargetLinearVelocity(targetVel.Linear);

        if (context.ActiveSectorExpirationSeconds.HasValue)
        {
            await _sectorPoolManager.SetExpirationFromNow(context.Sector, context.ActiveSectorExpirationSeconds.Value);
        }

        try
        {
            var npcConstructInfoOutcome = await _constructService.GetConstructInfoAsync(constructId);
            var npcConstructInfo = npcConstructInfoOutcome.Info;
            if (npcConstructInfo == null)
            {
                return;
            }

            var targetConstructExists = await _constructService.Exists(targetId);
            if (!targetConstructExists)
            {
                return;
            }

            var targeting = new TargetingConstructData
            {
                constructId = constructId,
                ownerId = new EntityId { playerId = prefab.DefinitionItem.OwnerId },
                constructName = npcConstructInfo.rData.name
            };

            if (selectedTarget.Distance <= 2 * DistanceHelpers.OneSuInMeters)
            {
                await _constructService.SendIdentificationNotification(
                    targetId,
                    targeting
                );

                if (context.HasAnyWeapons())
                {
                    await _constructService.SendAttackingNotification(
                        targetId,
                        targeting
                    );
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Identity Target");
        }

        if (!context.OverridePilotTakeOver)
        {
            try
            {
                await PilotingTakeOverAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to Takeover Ship");
            }
        }
    }

    private async Task PilotingTakeOverAsync()
    {
        if (!await _constructService.IsBeingControlled(constructId))
        {
            await _constructGrain.PilotingTakeOver(ModBase.Bot.PlayerId, true);
        }
    }

    private async Task<TargetMovePositionCalculationOutcome> CalculateTargetMovePosition(BehaviorContext context)
    {
        var targetConstructId = context.GetTargetConstructId();

        var effect = context.Effects.GetOrNull<ICalculateTargetMovePositionEffect>();
        if (effect == null || !targetConstructId.HasValue)
        {
            return TargetMovePositionCalculationOutcome.Invalid();
        }

        var targetMoveDistance = prefab.DefinitionItem.TargetDistance;
        if (context.DamageData.Weapons.Any())
        {
            targetMoveDistance =
                context.DamageData.GetHalfFalloffFiringDistance(context.DamageData.GetBestDamagingWeapon()!) *
                prefab.DefinitionItem.Mods.Weapon.OptimalDistance;
        }

        context.SetTargetMoveDistance(targetMoveDistance);

        var targetConstructTransformOutcome =
            await _constructService.GetConstructTransformAsync(targetConstructId.Value);
        if (targetConstructTransformOutcome.ConstructExists)
        {
            context.SetTargetPosition(targetConstructTransformOutcome.Position);
            if (context.Position.HasValue)
            {
                context.SetTargetDistance(
                    Math.Abs(targetConstructTransformOutcome.Position.Dist(context.Position.Value))
                );
            }
        }

        return await effect.GetTargetMovePosition(
            new ICalculateTargetMovePositionEffect.Params
            {
                InstigatorConstructId = constructId,
                InstigatorStartPosition = context.StartPosition,
                InstigatorPosition = context.Position,
                TargetMoveDistance = targetMoveDistance,
                TargetConstructId = targetConstructId,
                TargetConstructAcceleration = context.TargetAcceleration,
                TargetConstructLinearVelocity = context.TargetLinearVelocity,
                PredictionSeconds = context.CalculateMovementPredictionSeconds(),
                DeltaTime = context.DeltaTime
            });
    }
}