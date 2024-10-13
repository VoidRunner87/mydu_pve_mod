using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class AggressiveBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private IEnumerable<ElementId> _weaponsElements;
    private List<WeaponHandle> _weaponUnits;
    private IClusterClient _orleans;
    private IGameplayBank _bank;
    private ILogger<AggressiveBehavior> _logger;
    private IConstructService _constructService;

    private ElementId _coreUnitElementId;

    private bool _active = true;
    private bool _pveVoxelDamageEnabled;
    private IConstructElementsService _constructElementsService;

    public bool IsActive() => _active;

    public class WeaponHandle(ElementInfo elementInfo, WeaponUnit unit)
    {
        public ElementInfo ElementInfo { get; } = elementInfo;
        public WeaponUnit Unit { get; } = unit;
    }

    public BehaviorTaskCategory Category => BehaviorTaskCategory.HighPriority;

    public async Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _orleans = provider.GetOrleans();

        _constructElementsService = provider.GetRequiredService<IConstructElementsService>();

        _bank = provider.GetGameplayBank();

        _weaponsElements = await _constructElementsService.GetWeaponUnits(constructId);
        var elementInfos = await Task.WhenAll(
            _weaponsElements.Select(id => _constructElementsService.GetElement(constructId, id))
        );
        _weaponUnits = elementInfos
            .Select(ei => new WeaponHandle(ei, _bank.GetBaseObject<WeaponUnit>(ei)!))
            .Where(w => w.Unit is not StasisWeaponUnit) // TODO Implement Stasis later
            .ToList();

        _coreUnitElementId = await _constructElementsService.GetCoreUnit(constructId);

        _constructService = provider.GetRequiredService<IConstructService>();

        context.Properties.TryAdd("CORE_ID", _coreUnitElementId);

        _pveVoxelDamageEnabled = await context.ServiceProvider
            .GetRequiredService<IFeatureReaderService>()
            .GetBoolValueAsync("PVEVoxelDamage", false);

        _logger = provider.CreateLogger<AggressiveBehavior>();
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            _active = false;

            return;
        }

        var targetConstructId = context.GetTargetConstructId();

        if (!targetConstructId.HasValue)
        {
            return;
        }

        var provider = context.ServiceProvider;

        var npcShotGrain = _orleans.GetNpcShotGrain();

        var constructInfoOutcome = await _constructService.GetConstructInfoAsync(constructId);
        var constructInfo = constructInfoOutcome.Info;
        if (constructInfo == null)
        {
            return;
        }
        
        var constructPos = constructInfo.rData.position;

        if (targetConstructId is null or 0)
        {
            return;
        }

        var targetInfoOutcome = await _constructService.GetConstructInfoAsync(targetConstructId.Value);
        var targetInfo = targetInfoOutcome.Info;
        if (targetInfo == null)
        {
            return;
        }
        
        var targetSize = targetInfo.rData.geometry.size;

        if (targetInfo.mutableData.pilot.HasValue)
        {
            context.PlayerIds.Add(targetInfo.mutableData.pilot.Value);
        }

        var random = provider.GetRandomProvider()
            .GetRandom();

        // var hitPos = random.RandomDirectionVec3() * targetSize / 2;
        var hitPos = random.RandomDirectionVec3() * targetSize / 4;
        var constructSize = (ulong)constructInfo.rData.geometry.size;
        var targetPos = targetInfo.rData.position;

        if (_weaponUnits.Count == 0)
        {
            return;
        }
        
        var weapon = random.PickOneAtRandom(_weaponUnits);

        await ShootAndCycleAsync(
            new ShotContext(
                context,
                npcShotGrain,
                weapon,
                constructPos,
                constructSize,
                targetConstructId.Value,
                targetPos,
                hitPos,
                _weaponUnits.Count // One shot equivalent of all weapons for performance reasons
            )
        );
    }

    public class ShotContext(
        BehaviorContext behaviorContext,
        INpcShotGrain npcShotGrain,
        WeaponHandle weaponHandle,
        Vec3 constructPosition,
        ulong constructSize,
        ulong targetConstructId,
        Vec3 targetPosition,
        Vec3 hitPosition,
        int quantityModifier
    )
    {
        public BehaviorContext BehaviorContext { get; set; } = behaviorContext;
        public INpcShotGrain NpcShotGrain { get; set; } = npcShotGrain;
        public WeaponHandle WeaponHandle { get; set; } = weaponHandle;
        public Vec3 ConstructPosition { get; set; } = constructPosition;
        public ulong ConstructSize { get; set; } = constructSize;
        public ulong TargetConstructId { get; set; } = targetConstructId;
        public Vec3 TargetPosition { get; set; } = targetPosition;
        public Vec3 HitPosition { get; set; } = hitPosition;
        public int QuantityModifier { get; set; } = quantityModifier;
    }

    private const string ShotTotalDeltaTimePropName = $"{nameof(AggressiveBehavior)}_ShotTotalDeltaTime";

    private double GetShootTotalDeltaTime(BehaviorContext context)
    {
        if (context.Properties.TryGetValue(ShotTotalDeltaTimePropName, out var value))
        {
            return (double)value;
        }

        return 0;
    }

    private void SetShootTotalDeltaTime(BehaviorContext context, double value)
    {
        if (!context.Properties.TryAdd(ShotTotalDeltaTimePropName, value))
        {
            context.Properties[ShotTotalDeltaTimePropName] = value;
        }
    }

    private async Task SendShootModAction(ShotContext context)
    {
        var modManagerGrain = _orleans.GetModManagerGrain();

        await modManagerGrain.TriggerModAction(
            ModBase.Bot.PlayerId,
            new ModAction
            {
                constructId = constructId,
                modName = "Mod.DynamicEncounters",
                actionId = 1,
                payload = JsonConvert.SerializeObject(
                    new
                    {
                        context.TargetConstructId,
                        WeaponElementId = context.WeaponHandle.ElementInfo.elementId,
                    }
                )
            }
        );
    }

    private async Task ShootAndCycleAsync(ShotContext context)
    {
        var distance = (context.TargetPosition - context.ConstructPosition).Size();

        if (distance > 2 * DistanceHelpers.OneSuInMeters)
        {
            return;
        }
        
        var functionalWeaponCount = await _constructElementsService.GetFunctionalDamageWeaponCount(constructId);
        if (functionalWeaponCount <= 0)
        {
            return;
        }

        _logger.LogDebug("Construct {Construct} Functional Weapon Count {Count}", constructId, functionalWeaponCount);

        context.QuantityModifier = functionalWeaponCount;

        var random = context.BehaviorContext.ServiceProvider.GetRequiredService<IRandomProvider>()
            .GetRandom();

        var totalDeltaTime = GetShootTotalDeltaTime(context.BehaviorContext);
        totalDeltaTime += context.BehaviorContext.DeltaTime;

        SetShootTotalDeltaTime(context.BehaviorContext, totalDeltaTime);

        var handle = context.WeaponHandle;

        var w = handle.Unit;
        var mod = prefab.DefinitionItem.Mods;
        var cycleTime = w.baseCycleTime * mod.Weapon.CycleTime;

        if (totalDeltaTime < cycleTime)
        {
            return;
        }

        var isInSafeZone = await _constructService.IsInSafeZone(constructId);
        if (isInSafeZone)
        {
            return;
        }

        if (context.TargetConstructId > 0)
        {
            var targetInSafeZone = await _constructService.NoCache().IsInSafeZone(context.TargetConstructId);
            if (targetInSafeZone)
            {
                return;
            }
        }

        SetShootTotalDeltaTime(context.BehaviorContext, 0);

        if (prefab.DefinitionItem.AmmoItems.Count == 0)
        {
            prefab.DefinitionItem.AmmoItems = ["AmmoMissileLarge4"];
        }

        if (prefab.DefinitionItem.WeaponItems.Count == 0)
        {
            prefab.DefinitionItem.WeaponItems = ["WeaponMissileLargeAgile5"];
        }

        var ammoItem = random.PickOneAtRandom(prefab.DefinitionItem.AmmoItems);
        var weaponItem = random.PickOneAtRandom(prefab.DefinitionItem.WeaponItems);

        context.HitPosition = _pveVoxelDamageEnabled
            ? random.PickOneAtRandom(context.BehaviorContext.TargetElementPositions)
            : context.HitPosition;

        var sw = new Stopwatch();
        sw.Start();

        await context.NpcShotGrain.Fire(
            w.displayName,
            context.ConstructPosition,
            constructId,
            context.ConstructSize,
            context.TargetConstructId,
            context.TargetPosition,
            new SentinelWeapon
            {
                aoe = true,
                damage = w.baseDamage * mod.Weapon.Damage * context.QuantityModifier,
                range = 400000,
                aoeRange = 100000,
                baseAccuracy = w.baseAccuracy * mod.Weapon.Accuracy,
                effectDuration = 10,
                effectStrength = 10,
                falloffDistance = w.falloffDistance * mod.Weapon.FalloffDistance,
                falloffTracking = w.falloffTracking * mod.Weapon.FalloffTracking,
                fireCooldown = cycleTime,
                baseOptimalDistance = w.baseOptimalDistance * mod.Weapon.OptimalDistance,
                falloffAimingCone = w.falloffAimingCone * mod.Weapon.FalloffAimingCone,
                baseOptimalTracking = w.baseOptimalTracking * mod.Weapon.OptimalTracking,
                baseOptimalAimingCone = w.baseOptimalAimingCone * mod.Weapon.OptimalAimingCone,
                optimalCrossSectionDiameter = w.optimalCrossSectionDiameter,
                ammoItem = ammoItem,
                weaponItem = weaponItem
            },
            5,
            context.HitPosition
        );

        _logger.LogInformation("Construct {Construct} Shot Weapon. Took: {Time}ms {Weapon} / {Ammo}",
            constructId,
            sw.Elapsed.TotalMilliseconds,
            weaponItem,
            ammoItem
        );
    }
}