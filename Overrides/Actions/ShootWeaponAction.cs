using System;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Scenegraph;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides.Actions.Data;
using Mod.DynamicEncounters.Overrides.Common;
using Mod.DynamicEncounters.Overrides.Common.Helper;
using Newtonsoft.Json;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Overrides.Actions;

public class ShootWeaponAction(IServiceProvider provider) : IModActionHandler
{
    private readonly Random _random = new();
    private readonly IClusterClient _orleans = provider.GetRequiredService<IClusterClient>();
    private readonly IGameplayBank _bank = provider.GetRequiredService<IGameplayBank>();
    private readonly IScenegraph _sceneGraph = provider.GetRequiredService<IScenegraph>();
    private readonly ILogger _logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<ShootWeaponAction>();

    public async Task HandleAction(ulong playerId, ModAction action)
    {
        var shotData = JsonConvert.DeserializeObject<ShootWeaponData>(action.payload);
        var weapon = shotData.Weapon;

        var weaponDef = _bank.GetDefinition(weapon.weaponItem);
        if (weaponDef?.BaseObject is not WeaponUnit weaponUnit)
        {
            return;
        }

        var impactWorldPos = await _sceneGraph.ResolveWorldLocation(
            new RelativeLocation
            {
                constructId = shotData.TargetConstructId,
                position = shotData.LocalHitPosition
            });
        var originWorldPos = await _sceneGraph.GetConstructCenterWorldPosition(shotData.ShooterConstructId);

        if (impactWorldPos.position.Dist(originWorldPos) > weapon.baseOptimalDistance + weapon.falloffDistance * 5)
        {
            // TODO Far miss
            return;
        }

        ShotOutcome shotOutcome;
        if (weaponUnit is StasisWeaponUnit)
        {
            shotOutcome = await Handle(
                new StasisShotParams
                {
                    ShootWeaponData = shotData,
                    ShotImpactWorldPosition = impactWorldPos.position,
                    ShotOriginWorldPosition = originWorldPos
                });
        }
        else
        {
            shotOutcome = await Handle(
                new DamagingShotParams
                {
                    WeaponUnit = weaponUnit,
                    ShootWeaponData = shotData,
                    ShotImpactWorldPosition = impactWorldPos.position,
                    ShotOriginWorldPosition = originWorldPos
                });
        }

        if (!shotOutcome.Success)
        {
            _logger.LogError("Failed to preform Shoot Weapon: {Message}", shotOutcome.Message);
        }
        else
        {
            _logger.LogInformation("Shot Operation Success. Hit: {Hit}", shotOutcome.Hit);
        }
    }

    private async Task<ShotOutcome> Handle(StasisShotParams @params)
    {
        var shooterPosition = @params.ShootWeaponData.ShooterPosition;
        var targetPosition =
            await _sceneGraph.GetConstructCenterWorldPosition(@params.ShootWeaponData.TargetConstructId);
        var weapon = @params.ShootWeaponData.Weapon;

        var result = new WeaponFireResult();

        var isValidStasisHit = shooterPosition.Dist(targetPosition) <= weapon.range;
        result.hit = isValidStasisHit;

        if (isValidStasisHit)
        {
            var targetConstructGrain = _orleans.GetConstructGrain(@params.ShootWeaponData.TargetConstructId);

            await targetConstructGrain.UpdateConstructInfo(
                new ConstructInfoUpdate
                {
                    additionalMaxSpeedDebuf = new MaxSpeedDebuf
                    {
                        until = DateTime.UtcNow.AddSeconds(weapon.effectDuration).ToNQTimePoint(),
                        value = weapon.effectStrength
                    }
                });

            return ShotOutcome.HitTarget(
                new WeaponImpact(),
                result
            );
        }

        return ShotOutcome.Miss(
            CalculateMissImpact(
                @params.ShotOriginWorldPosition,
                @params.ShotImpactWorldPosition,
                16d,
                0.5d
            ),
            result
        );
    }

    private async Task<ShotOutcome> Handle(DamagingShotParams @params)
    {
        var shooterConstructId = @params.ShootWeaponData.ShooterConstructId;
        var shooterPosition = @params.ShootWeaponData.ShooterPosition;

        var shooterConstructGrain = _orleans.GetConstructInfoGrain(@params.ShootWeaponData.ShooterConstructId);
        var shooterInfo = await shooterConstructGrain.Get();
        var shooterRot = shooterInfo.rData.rotation;
        
        var targetConstructId = @params.ShootWeaponData.TargetConstructId;
        var targetPosition = await _sceneGraph
            .GetConstructCenterWorldPosition(@params.ShootWeaponData.TargetConstructId);
        var npcCenterPosition = await _sceneGraph
            .GetConstructCenterWorldPosition(@params.ShootWeaponData.ShooterConstructId);

        var directServiceGrain = _orleans.GetDirectServiceGrain();
        var shooterConstructElementsGrain = _orleans.GetConstructElementsGrain(shooterConstructId);
        var weapons = await shooterConstructElementsGrain.GetElementsOfType<WeaponUnit>();
        var firstWeapon = weapons.First();
        var firstWeaponElementInfo = await shooterConstructElementsGrain.GetElement(firstWeapon);

        var shooterWeaponLocalPos = firstWeaponElementInfo.position;
        var shooterWeaponPos = VectorMathHelper.CalculateWorldPosition(
            shooterWeaponLocalPos.ToVector3(),
            shooterPosition.ToVector3(),
            shooterRot.ToQuat()
        );
        
        var targetConstructGrain = _orleans.GetConstructGrain(targetConstructId);
        
        var weapon = @params.ShootWeaponData.Weapon;

        var weaponImpact = new WeaponImpact();

        var hitRatio = await CalculateHitRatio(
            shooterConstructId,
            targetConstructId,
            npcCenterPosition,
            targetPosition,
            weapon,
            @params.ShotImpactWorldPosition,
            @params.ShootWeaponData.CrossSection
        );
        
        var hitPositionWorld = await _sceneGraph.ResolveWorldLocation(new RelativeLocation
        {
            constructId = targetConstructId,
            position = @params.ShootWeaponData.LocalHitPosition
        });

        var num = _random.NextDouble();

        var isHit = num <= hitRatio;

        var result = new WeaponFireResult
        {
            hit = isHit
        };

        if (isHit)
        {
            var targetConstructFightGrain = _orleans.GetConstructFightGrain(targetConstructId);
            var targetConstructDamageGrain = _orleans.GetConstructDamageElementsGrain(targetConstructId);

            var hit = await targetConstructFightGrain.ConstructTakeHit(new WeaponShotPower
            {
                ammoType = _bank.IdFor(weapon.ammoItem),
                power = weapon.damage,
                originPlayerId = @params.ShootWeaponData.ShooterPlayerId,
                originConstructId = shooterConstructId
            });

            result.coreUnitStressDamage = hit.coreUnitStressDamage;

            if (hit.effect == ShieldHitEffect.ShieldAbsorbedHit)
            {
                result.shieldDamage = hit.shieldDamage;
                result.rawShieldDamage = hit.rawShieldDamage;
            }
            else
            {
                result.coreUnitDestroyed = hit.coreUnitStressDestroyed;

                var deathInfoPvp = new PlayerDeathInfoPvPData
                {
                    weaponId = 0,
                    weaponTypeId = _bank.IdFor(weapon.weaponItem),
                    constructId = shooterConstructId,
                    constructName = @params.ShootWeaponData.ShooterName,
                    playerId = @params.ShootWeaponData.ShooterPlayerId,
                    playerName = "unknown",
                    ownerId = new EntityId { playerId = @params.ShootWeaponData.ShooterPlayerId }
                };

                if (hit.coreUnitStressDestroyed)
                {
                    await targetConstructDamageGrain.TriggerCoreUnitStressDestruction(deathInfoPvp);
                }

                var playerListAndPosition = await targetConstructGrain.GetKillablePlayerListAndPosition();

                var weaponFire = new WeaponFire
                {
                    constructId = shooterConstructId,
                    targetId = targetConstructId,
                    impactPoint = @params.ShootWeaponData.LocalHitPosition,
                    bboxCenterLocal = @params.ShootWeaponData.LocalHitPosition,
                    bboxSizeLocal = new Vec3
                    {
                        x = 16.0,
                        y = 16.0,
                        z = 16.0
                    },
                    crossSection = 5.0
                };

                var ammoDef = _bank.GetDefinition(weapon.ammoItem);
                if (ammoDef == null)
                {
                    return ShotOutcome.InvalidAmmoDefinition(weapon.ammoItem);
                }

                if (ammoDef.BaseObject is not Ammo ammo)
                {
                    return ShotOutcome.InvalidAmmoDefinition(weapon.ammoItem);
                }

                VoxelInternalEditResults voxelResult;
                if (@params.ShootWeaponData.DamagesVoxel)
                {
                    voxelResult = await directServiceGrain.MakeVoxelDamages(
                        weaponFire,
                        ammo,
                        weapon.damage,
                        playerListAndPosition
                    );
                }
                else
                {
                    voxelResult = new VoxelInternalEditResults();
                }

                var deathInfo = new PlayerDeathInfo
                {
                    reason = DeathReason.WeaponShot,
                    pvpData = deathInfoPvp
                };

                if (voxelResult.damageOutput != null)
                {
                    var damageResult = await targetConstructDamageGrain.ApplyPvpElementsDamage(
                        voxelResult.damageOutput.elements,
                        deathInfoPvp
                    );
                    result.totalDamage = voxelResult.damageOutput.totalDamage;

                    foreach (var player in voxelResult.damageOutput.deadPlayers)
                    {
                        var playerGrain = _orleans.GetPlayerGrain(player);
                        var playerInfo = await playerGrain.GetPlayerInfo();

                        await playerGrain.PlayerDieOperation(deathInfo);
                        var namedEntityList = result.playersKilled;
                        var namedEntity1 = new NamedEntity
                        {
                            id = new EntityId
                            {
                                playerId = player,
                            },
                            name = playerInfo.name
                        };

                        namedEntityList.Add(namedEntity1);
                    }

                    result.coreUnitDestroyed |= damageResult.CoreUnitDestroyed;
                    result.destroyedElementTypes = damageResult.broken
                        .Select((Func<(ElementId, ulong), ulong>)(t => t.Item2)).ToList();
                }
            }
            
            weaponImpact.ImpactPositionWorld = @params.ShotImpactWorldPosition;
            weaponImpact.ImpactPositionLocal = @params.ShootWeaponData.LocalHitPosition;
            weaponImpact.TargetId = targetConstructId;
                
            var hitWeaponShot = new WeaponShot
            {
                id = (ulong)TimePoint.Now().networkTime,
                originConstructId = shooterConstructId,
                originPositionWorld = shooterWeaponPos.ToNqVec3(),
                originPositionLocal = shooterWeaponLocalPos,
                targetConstructId = targetConstructId,
                weaponType = _bank.IdFor(weapon.weaponItem),
                ammoType = _bank.IdFor(weapon.ammoItem),
                impactPositionWorld = hitPositionWorld.position,
                impactPositionLocal = @params.ShootWeaponData.LocalHitPosition,
                shieldDamage = result.shieldDamage,
                rawShieldDamage = result.rawShieldDamage,
                coreUnitDestroyed = result.coreUnitDestroyed,
                impactElementType = 3
            };

            await directServiceGrain.PropagateShotImpact(hitWeaponShot);

            return ShotOutcome.HitTarget(
                weaponImpact,
                result
            );
        }

        var targetConstructInfoGrain = _orleans.GetConstructInfoGrain(targetConstructId);
        var targetInfo = await targetConstructInfoGrain.Get();

        var missImpact = CalculateMissImpact(
            @params.ShotOriginWorldPosition,
            targetPosition,
            targetInfo.rData.geometry.size / 0.5d,
            num - hitRatio
        );
        
        var missWeaponShot = new WeaponShot
        {
            id = (ulong)TimePoint.Now().networkTime,
            originConstructId = shooterConstructId,
            originPositionWorld = shooterWeaponPos.ToNqVec3(),
            originPositionLocal = shooterWeaponLocalPos,
            targetConstructId = targetConstructId,
            weaponType = _bank.IdFor(weapon.weaponItem),
            ammoType = _bank.IdFor(weapon.ammoItem),
            impactPositionWorld = missImpact.ImpactPositionWorld,
            impactPositionLocal = missImpact.ImpactPositionLocal,
            shieldDamage = result.shieldDamage,
            rawShieldDamage = result.rawShieldDamage,
            coreUnitDestroyed = result.coreUnitDestroyed,
            impactElementType = 3
        };
        
        await directServiceGrain.PropagateShotImpact(missWeaponShot);

        return ShotOutcome.Miss(
            missImpact,
            result
        );
    }

    private async Task<double> CalculateHitRatio(
        ulong npcConstructId,
        ulong targetConstructId,
        Vec3 weaponWorldLocation,
        Vec3 target,
        SentinelWeapon weaponUnit,
        Vec3 impactWorldLocation,
        double crossSection)
    {
        var targetConstructGrain = _orleans.GetConstructGrain(targetConstructId);
        var targetVelocity = await targetConstructGrain.GetConstructVelocity();

        var distance = weaponWorldLocation.Dist(impactWorldLocation);
        var accuracy = weaponUnit.baseAccuracy;
        var angleOptimalValue = weaponUnit.baseOptimalAimingCone;
        var angleFallOffValue = weaponUnit.falloffAimingCone;
        var angleFactor = ComputeFactor(0.0, angleOptimalValue, angleFallOffValue);
        var distanceOptimalValue = weaponUnit.baseOptimalDistance;
        var distanceFallOffValue = weaponUnit.falloffDistance;
        var distanceFactor = ComputeFactor(distance, distanceOptimalValue, distanceFallOffValue);
        var degrees = ComputeAngularVelocity(
            impactWorldLocation - weaponWorldLocation,
            targetVelocity.velocity,
            new Vector3D()
        ).Degrees;
        var baseOptimalTracking = weaponUnit.baseOptimalTracking;
        var falloffTracking = weaponUnit.falloffTracking;
        var factor = ComputeFactor(degrees, baseOptimalTracking, falloffTracking);
        var num1 = weaponUnit.optimalCrossSectionDiameter * 0.5;
        var num2 = Math.Min(1.0, Math.Sqrt(crossSection / (num1 * num1 * Math.PI)) * (1.0 - factor) + factor);

        return accuracy * num2 * angleFactor * distanceFactor * factor;
    }

    private static double ComputeFactor(
        double value,
        double optimalValue,
        double falloffValue,
        double factorValue = 1.0)
    {
        return Math.Pow(0.5, factorValue * Math.Pow(Math.Max(0.0, value - optimalValue) / falloffValue, 2.0));
    }

    private static Angle ComputeAngularVelocity(
        Vector3D target,
        Vector3D relativeVelocity,
        Vector3D localAngularVelocity)
    {
        return Angle.FromRadians((target.CrossProduct(relativeVelocity) / (target.Length * target.Length) -
                                  localAngularVelocity).Length);
    }

    private WeaponImpact CalculateMissImpactSimple()
    {
        return new WeaponImpact
        {
            ImpactPositionWorld = _random.RandomDirectionVec3() * 2000,
            ImpactPositionLocal = _random.RandomDirectionVec3() * 2000,
        };
    }
    
    private WeaponImpact CalculateMissImpact(
        Vec3 origin,
        Vec3 target,
        double size,
        double missRange)
    {
        Vector3D vector3D1 = target - origin;
        var orthogonal = vector3D1.Orthogonal;
        var unitVector3D1 = vector3D1.CrossProduct(orthogonal).Normalize();
        var num = _random.NextDouble() * (2.0 * Math.PI);
        var unitVector3D2 = (Math.Cos(num) * orthogonal + Math.Sin(num) * unitVector3D1).Normalize();
        var vector3D2 = size * (0.5 + _random.NextDouble() * missRange) * unitVector3D2;
        var vector3D3 = size * (0.5 + _random.NextDouble() * missRange) * vector3D1.Normalize();
    
        return new WeaponImpact
        {
            ImpactPositionWorld = (Vector3D)origin + vector3D1 + vector3D3 + vector3D2,
            ImpactPositionLocal = (Vector3D)origin + vector3D1 + vector3D3 + vector3D2
        };
    }

    private class StasisShotParams
    {
        public ShootWeaponData ShootWeaponData { get; set; }
        public Vec3 ShotOriginWorldPosition { get; set; }
        public Vec3 ShotImpactWorldPosition { get; set; }
    }

    private class DamagingShotParams
    {
        public ShootWeaponData ShootWeaponData { get; set; }
        public WeaponUnit WeaponUnit { get; set; }
        public Vec3 ShotOriginWorldPosition { get; set; }
        public Vec3 ShotImpactWorldPosition { get; set; }
    }

    public class WeaponImpact
    {
        public Vec3 ImpactPositionLocal { get; set; }

        public Vec3 ImpactPositionWorld { get; set; }

        public ulong ImpactVoxelMaterialId { get; set; }

        public ulong ImpactElementId { get; set; }

        public ulong ImpactElementType { get; set; }

        public ulong TargetId { get; set; }
    }

    public class ShotOutcome
    {
        public bool Success { get; init; }
        public bool Hit { get; init; }
        public WeaponImpact Impact { get; init; }
        public WeaponFireResult FireResult { get; init; }

        public string Message { get; set; }

        public static ShotOutcome HitTarget(WeaponImpact impact, WeaponFireResult fireResult)
            => new() { Success = true, Hit = true, Impact = impact, FireResult = fireResult };

        public static ShotOutcome Miss(WeaponImpact impact, WeaponFireResult fireResult)
            => new() { Success = true, Hit = true, Impact = impact, FireResult = fireResult };

        public static ShotOutcome InvalidAmmoDefinition(string ammo)
            => new() { Success = false, Hit = true, Message = $"Invalid ammo def '{ammo}'" };
    }
}