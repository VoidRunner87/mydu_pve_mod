using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Helpers.DU;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class AggressiveBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private List<ElementId> _weaponsElements;
    private List<WeaponHandle> _weaponUnits;
    private IClusterClient _orleans;
    private IGameplayBank _bank;
    private IConstructGrain _constructGrain;
    private ILogger<AggressiveBehavior> _logger;
    private IConstructElementsGrain _constructElementsGrain;

    private ElementId _coreUnitElementId;
    
    private bool _active = true;

    public bool IsActive() => _active;

    public class WeaponHandle(ElementInfo elementInfo, WeaponUnit unit)
    {
        public ElementInfo ElementInfo { get; } = elementInfo;
        public WeaponUnit Unit { get; } = unit;
    }

    public async Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _orleans = provider.GetOrleans();

        _constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);

        _bank = provider.GetGameplayBank();

        _weaponsElements = await _constructElementsGrain.GetElementsOfType<WeaponUnit>();
        var elementInfos = await Task.WhenAll(
            _weaponsElements.Select(_constructElementsGrain.GetElement)
        );
        _weaponUnits = elementInfos
            .Select(ei => new WeaponHandle(ei, _bank.GetBaseObject<WeaponUnit>(ei)!))
            .Where(w => w.Unit is not StasisWeaponUnit) // TODO Implement Stasis later
            .ToList();

        _coreUnitElementId = (await _constructElementsGrain.GetElementsOfType<CoreUnit>()).SingleOrDefault();

        _constructGrain = _orleans.GetConstructGrain(constructId);
        
        context.ExtraProperties.TryAdd("CORE_ID", _coreUnitElementId);
        
        context.IsAlive = _coreUnitElementId.elementId > 0;
        _active = context.IsAlive;
        
        _logger = provider.CreateLogger<AggressiveBehavior>();
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
        
        var coreUnit = await _constructElementsGrain.GetElement(_coreUnitElementId);

        if (coreUnit.IsCoreStressHigh())
        {
            await context.NotifyCoreStressHighAsync(new BehaviorEventArgs(constructId, prefab, context));
        }
        
        var provider = context.ServiceProvider;

        var constructInfoGrain = _orleans.GetConstructInfoGrain(constructId);
        var npcShotGrain = _orleans.GetNpcShotGrain();

        var constructInfo = await constructInfoGrain.Get();
        var constructPos = constructInfo.rData.position;

        if (context.TargetConstructId is null or 0)
        {
            return;
        }
        
        var targetInfoGrain = _orleans.GetConstructInfoGrain(new ConstructId{constructId = context.TargetConstructId.Value});
        var targetInfo = await targetInfoGrain.Get();
        var targetSize = targetInfo.rData.geometry.size;
        
        if (targetInfo.mutableData.pilot.HasValue)
        {
            context.PlayerIds.TryAdd(targetInfo.mutableData.pilot.Value, targetInfo.mutableData.pilot.Value);
        }

        if (constructInfo.IsShieldLowerThanHalf())
        {
            await context.NotifyShieldHpHalfAsync(new BehaviorEventArgs(constructId, prefab, context));
        }
        
        if (constructInfo.IsShieldLowerThan25())
        {
            await context.NotifyShieldHpLowAsync(new BehaviorEventArgs(constructId, prefab, context));
        }
        
        if (constructInfo.IsShieldDown())
        {
            await context.NotifyShieldHpDownAsync(new BehaviorEventArgs(constructId, prefab, context));
        }
        
        var random = provider.GetRandomProvider()
            .GetRandom();

        // var hitPos = random.RandomDirectionVec3() * targetSize / 2;
        var hitPos = random.RandomDirectionVec3() * targetSize / 4;
        var constructSize = (ulong)constructInfo.rData.geometry.size;
        var targetPos = targetInfo.rData.position;

        var weapon = random.PickOneAtRandom(_weaponUnits);

        await ShootAndCycleAsync(
            new ShotContext(
                context,
                npcShotGrain,
                weapon,
                constructPos,
                constructSize,
                context.TargetConstructId.Value,
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
        public int QuantityModifier { get; } = quantityModifier;
    }

    private const string ShotTotalDeltaTimePropName = $"{nameof(AggressiveBehavior)}_ShotTotalDeltaTime";
    
    private double GetShootTotalDeltaTime(BehaviorContext context)
    {
        if (context.ExtraProperties.TryGetValue(ShotTotalDeltaTimePropName, out var value))
        {
            return (double)value;
        }

        return 0;
    }

    private void SetShootTotalDeltaTime(BehaviorContext context, double value)
    {
        if (!context.ExtraProperties.TryAdd(ShotTotalDeltaTimePropName, value))
        {
            context.ExtraProperties[ShotTotalDeltaTimePropName] = value;
        }
    }

    private async Task ShootAndCycleAsync(ShotContext context)
    {
        var random = context.BehaviorContext.ServiceProvider.GetRequiredService<IRandomProvider>()
            .GetRandom();
        
        var totalDeltaTime = GetShootTotalDeltaTime(context.BehaviorContext);
        totalDeltaTime += context.BehaviorContext.DeltaTime;
        
        SetShootTotalDeltaTime(context.BehaviorContext, totalDeltaTime);
        
        var handle = context.WeaponHandle;

        // TODO check if weapon is destroyed
        var elementInfo = await _constructElementsGrain.GetElement(handle.ElementInfo.elementId);

        var w = handle.Unit;
        var mod = prefab.DefinitionItem.Mods;
        var cycleTime = w.baseCycleTime * mod.Weapon.CycleTime;

        if (totalDeltaTime < cycleTime)
        {
            return;
        }

        var isInSafeZone = await _constructGrain.IsInSafeZone();
        if (isInSafeZone)
        {
            return;
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
    }
}