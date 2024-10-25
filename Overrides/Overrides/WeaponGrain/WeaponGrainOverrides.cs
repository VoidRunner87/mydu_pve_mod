using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Business;
using Backend.Database;
using Backend.Scenegraph;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using NQutils.Exceptions;
using NQutils.Sql;
using Orleans;
using Services;
using ErrorCode = NQ.ErrorCode;

namespace Mod.DynamicEncounters.Overrides.Overrides.WeaponGrain;

public class WeaponGrainOverrides(IServiceProvider provider)
{
    private static readonly MemoryCache TimerCache = new(
        new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromSeconds(1 / 30d)
        }
    );

    public async Task<WeaponFireResult> WeaponFireOnce(
        IIncomingGrainCallContext context,
        PlayerId playerId,
        WeaponFire weaponFire
    )
    {
        var bank = provider.GetRequiredService<IGameplayBank>();
        var sql = provider.GetRequiredService<ISql>();

        var weaponInfo = await sql.GetElement(weaponFire.weaponId, fetchLinks: false);
        var weaponUnit = bank.GetBaseObject<WeaponUnit>(weaponInfo.elementType);
        if (weaponUnit == null)
        {
            throw new BusinessException(ErrorCode.WeaponShotKillInvalid);
        }

        if (weaponUnit is StasisWeaponUnit)
        {
            return await WeaponFireStasis(context, playerId, weaponFire);
        }

        return await WeaponFireDamaging(context, playerId, weaponFire);
    }

    private async Task<WeaponFireResult> WeaponFireStasis(
        IIncomingGrainCallContext context,
        PlayerId playerId,
        WeaponFire weaponFire
    )
    {
        var logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<WeaponGrainOverrides>();

        var random = new Random();
        var constructId = weaponFire.constructId;

        var sql = provider.GetRequiredService<ISql>();
        var orleans = provider.GetRequiredService<IClusterClient>();
        var bank = provider.GetRequiredService<IGameplayBank>();
        var sceneGraph = provider.GetRequiredService<IScenegraph>();
        var weaponAim = provider.GetRequiredService<IElementWeaponAim>();

        var playerGrain = orleans.GetPlayerGrain(playerId);

        var constructGrain = orleans.GetConstructGrain(weaponFire.constructId);
        var constructFightGrain = orleans.GetConstructFightGrain(weaponFire.constructId);

        var targetConstructGrain = orleans.GetConstructGrain(weaponFire.targetId);
        var targetConstructFightGrain = orleans.GetConstructFightGrain(weaponFire.targetId);

        var weaponInfo = await sql.GetElement(weaponFire.weaponId, fetchLinks: false);
        var directServiceGrain = orleans.GetDirectServiceGrain();

        var weaponUnit = bank.GetBaseObject<WeaponUnit>(weaponInfo.elementType);
        if (weaponUnit == null)
        {
            logger.LogError("Weapon Unit Null");
            throw new BusinessException(ErrorCode.WeaponShotKillInvalid);
        }

        var stasisWeaponUnit = (StasisWeaponUnit)weaponUnit;

        var ammoTypeId =
            (ulong)ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties, weaponUnit,
                WeaponUnit.d_ammoType);
        var ammoDef = bank.GetBaseObject<Ammo>(ammoTypeId);

        if (ammoDef == null)
        {
            logger.LogError("Ammo Def Null");
            throw new BusinessException(ErrorCode.WeaponShotKillInvalid);
        }

        if (!ConstructId.IsUserConstruct(weaponFire.targetId))
            throw new BusinessException(ErrorCode.InvalidConstructId, "Can only shoot between two User Construct");

        if (TimerCache.TryGetValue(new WeaponShotTimerKey(weaponFire.constructId, weaponInfo.elementId), out _))
            throw new BusinessException(ErrorCode.WeaponNotReady, "Weapon is on cooldown");

        // if (weaponGrain.State.Target == null || (ConstructId)weaponFire.targetId != weaponGrain.State.Target.Target)
        //     throw new BusinessException(ErrorCode.AttackSequenceNotStarted, "Can't shoot as not attacking");

        if (await constructGrain.IsInSafeZone())
            throw new BusinessException(ErrorCode.ConstructInvalidPosition, "Cannot fire from safe zone");

        if (await targetConstructGrain.IsInSafeZone())
            throw new BusinessException(ErrorCode.ConstructInvalidPosition, "Cannot fire to safe zone");

        var ammoCount = ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties,
            weaponUnit, WeaponUnit.d_ammoCount);

        if (ammoCount <= 0L)
            throw new BusinessException(ErrorCode.ElementPropertyInvalid, "Weapon has non-positive 'ammoCount'");
        if (ammoDef == null)
            throw new BusinessException(ErrorCode.ElementPropertyInvalid, "Weapon has invalid 'ammoType'");

        await CheckSeat(provider, orleans, weaponFire, playerId, weaponFire.seatId);

        await StartCycleTimer(orleans, weaponFire.constructId, playerId, weaponInfo, weaponUnit);

        var num1 = ammoCount - 1L;

        await SetDynamicProperty(provider, weaponFire, WeaponUnit.d_ammoCount, num1);

        await constructFightGrain.RefreshPvpTimer();
        await targetConstructFightGrain.RefreshPvpTimer();

        var weaponWorldLocation = await sceneGraph.ResolveWorldLocation(new RelativeLocation()
        {
            constructId = weaponFire.constructId,
            position = weaponInfo.position,
            rotation = weaponInfo.rotation
        });
        var constructOwner = await constructGrain.GetOwner();

        var bboxCenterWorld = await sceneGraph.ResolveWorldLocation(new RelativeLocation()
        {
            constructId = weaponFire.targetId,
            position = weaponFire.bboxCenterLocal
        });
        var playerName = (await playerGrain.GetPlayerInfo()).name;
        var result = new WeaponFireResult
        {
            constructId = weaponFire.constructId
        };
        var weaponImpact1 = new WeaponImpact();

        var weaponRay = weaponAim.GetRay(weaponInfo.elementType);

        ConstructSpeedConfig speedConfig = bank.GetBaseObject<ConstructSpeedConfig>();
        double totalMass = await targetConstructGrain.GetTotalMass();
        var range = stasisWeaponUnit.RangeMax;
        if (totalMass <= speedConfig.heavyConstructMass)
        {
            double num2 = (stasisWeaponUnit.RangeMin - stasisWeaponUnit.RangeMax) /
                          (1.0 - 1.0 / (stasisWeaponUnit.RangeCurvature + 1.0));
            range = stasisWeaponUnit.RangeMin - num2 +
                    num2 / (stasisWeaponUnit.RangeCurvature * totalMass / speedConfig.heavyConstructMass + 1.0);
        }

        double num3 = (await CalculateDistance(sceneGraph, weaponRay, weaponFire, weaponInfo)).Item1;
        if (num3 > range * 3.0)
        {
            result.hit = false;
            weaponImpact1 = CalculateMissImpact(random, weaponWorldLocation.position, bboxCenterWorld.position,
                ((Vector3D)weaponFire.bboxSizeLocal).Length * 0.5, 0.5);
        }
        else
        {
            result.hit = true;
            double num4 = Math.Pow(0.5, Math.Max(num3 - range, 0.0) / range) * stasisWeaponUnit.effectStrength;
            double propertyOrDefault = ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties,
                stasisWeaponUnit, StasisWeaponUnit.d_effectDuration);
            await targetConstructGrain.UpdateConstructInfo(
                new ConstructInfoUpdate
                {
                    additionalMaxSpeedDebuf = new MaxSpeedDebuf()
                    {
                        until = DateTime.UtcNow.AddSeconds(propertyOrDefault).ToNQTimePoint(),
                        value = num4
                    }
                });
        }

        if (result.hit)
        {
            weaponImpact1.ImpactPositionLocal = weaponFire.impactPoint;
            weaponImpact1.TargetId = weaponFire.targetId;
            weaponImpact1.ImpactElementId = weaponFire.impactElementId;
            weaponImpact1.ImpactElementType = weaponFire.impactElementType;
            weaponImpact1.ImpactVoxelMaterialId = weaponFire.impactVoxelMaterialId;
        }

        var shot = new WeaponShot
        {
            id = (ulong)TimePoint.Now().networkTime,
            originConstructId = constructId,
            weaponId = weaponFire.weaponId,
            weaponType = weaponInfo.elementType,
            ammoType = ammoTypeId,
            originPositionLocal = weaponInfo.position,
            originPositionWorld = weaponWorldLocation.position,
            targetConstructId = weaponImpact1.TargetId,
            impactPositionLocal = weaponImpact1.ImpactPositionLocal,
            impactPositionWorld = weaponImpact1.ImpactPositionWorld,
            impactVoxelMaterialId = weaponImpact1.ImpactVoxelMaterialId,
            impactElementId = weaponImpact1.ImpactElementId,
            impactElementType = weaponImpact1.ImpactElementType,
            shieldDamage = result.shieldDamage,
            rawShieldDamage = result.rawShieldDamage,
            coreUnitDestroyed = result.coreUnitDestroyed
        };

        await directServiceGrain.PropagateShotImpact(shot);

        return result;
    }

    private async Task<WeaponFireResult> WeaponFireDamaging(
        IIncomingGrainCallContext context,
        PlayerId playerId,
        WeaponFire weaponFire
    )
    {
        var logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<WeaponGrainOverrides>();

        var random = new Random();
        var constructId = weaponFire.constructId;

        var sql = provider.GetRequiredService<ISql>();
        var orleans = provider.GetRequiredService<IClusterClient>();
        var bank = provider.GetRequiredService<IGameplayBank>();
        var sceneGraph = provider.GetRequiredService<IScenegraph>();
        var weaponAim = provider.GetRequiredService<IElementWeaponAim>();

        var playerGrain = orleans.GetPlayerGrain(playerId);

        var constructGrain = orleans.GetConstructGrain(weaponFire.constructId);
        var constructFightGrain = orleans.GetConstructFightGrain(weaponFire.constructId);

        var targetConstructGrain = orleans.GetConstructGrain(weaponFire.targetId);
        var targetConstructFightGrain = orleans.GetConstructFightGrain(weaponFire.targetId);

        var weaponInfo = await sql.GetElement(weaponFire.weaponId, fetchLinks: false);
        var directServiceGrain = orleans.GetDirectServiceGrain();

        var weaponUnit = bank.GetBaseObject<WeaponUnit>(weaponInfo.elementType);
        if (weaponUnit == null)
        {
            logger.LogError("Weapon Unit Null");
            throw new BusinessException(ErrorCode.WeaponShotKillInvalid);
        }

        var ammoTypeId =
            (ulong)ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties, weaponUnit,
                WeaponUnit.d_ammoType);
        var ammoDef = bank.GetBaseObject<Ammo>(ammoTypeId);

        if (ammoDef == null)
        {
            logger.LogError("Ammo Def Null");
            throw new BusinessException(ErrorCode.WeaponShotKillInvalid);
        }

        // if (playerId != 4)
        // {
        //     await context.Invoke();
        //     return (WeaponFireResult)context.Result;
        // }

        if (!ConstructId.IsUserConstruct(weaponFire.targetId))
            throw new BusinessException(ErrorCode.InvalidConstructId, "Can only shoot between two User Construct");

        if (TimerCache.TryGetValue(new WeaponShotTimerKey(weaponFire.constructId, weaponInfo.elementId), out _))
            throw new BusinessException(ErrorCode.WeaponNotReady, "Weapon is on cooldown");

        // if (weaponGrain.State.Target == null || (ConstructId)weaponFire.targetId != weaponGrain.State.Target.Target)
        //     throw new BusinessException(ErrorCode.AttackSequenceNotStarted, "Can't shoot as not attacking");

        if (await constructGrain.IsInSafeZone())
            throw new BusinessException(ErrorCode.ConstructInvalidPosition, "Cannot fire from safe zone");

        if (await targetConstructGrain.IsInSafeZone())
            throw new BusinessException(ErrorCode.ConstructInvalidPosition, "Cannot fire to safe zone");

        var ammoCount = ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties,
            weaponUnit, WeaponUnit.d_ammoCount);

        if (ammoCount <= 0L)
            throw new BusinessException(ErrorCode.ElementPropertyInvalid, "Weapon has non-positive 'ammoCount'");
        if (ammoDef == null)
            throw new BusinessException(ErrorCode.ElementPropertyInvalid, "Weapon has invalid 'ammoType'");

        await CheckSeat(provider, orleans, weaponFire, playerId, weaponFire.seatId);

        await StartCycleTimer(orleans, weaponFire.constructId, playerId, weaponInfo, weaponUnit);

        var num1 = ammoCount - 1L;

        await SetDynamicProperty(provider, weaponFire, WeaponUnit.d_ammoCount, num1);

        await constructFightGrain.RefreshPvpTimer();
        await targetConstructFightGrain.RefreshPvpTimer();

        var weaponWorldLocation = await sceneGraph.ResolveWorldLocation(new RelativeLocation()
        {
            constructId = weaponFire.constructId,
            position = weaponInfo.position,
            rotation = weaponInfo.rotation
        });
        var constructOwner = await constructGrain.GetOwner();

        var bboxCenterWorld = await sceneGraph.ResolveWorldLocation(new RelativeLocation()
        {
            constructId = weaponFire.targetId,
            position = weaponFire.bboxCenterLocal
        });
        var playerName = (await playerGrain.GetPlayerInfo()).name;
        var result = new WeaponFireResult
        {
            constructId = weaponFire.constructId
        };
        var weaponImpact1 = new WeaponImpact();

        var weaponRay = weaponAim.GetRay(weaponInfo.elementType);

        logger.LogInformation("Weapon Ray: {Start} - {End}", weaponRay.start, weaponRay.end);

        var impactWorldTransform = await sceneGraph.ResolveWorldLocation(new RelativeLocation
        {
            constructId = weaponFire.targetId,
            position = weaponFire.impactPoint
        });

        var hitRatio = await CalculateHitRatio(
            provider,
            playerId,
            orleans,
            weaponRay,
            weaponUnit,
            weaponFire,
            weaponInfo,
            ammoDef,
            weaponWorldLocation,
            impactWorldTransform
        );
        var num5 = random.NextDouble();

        logger.LogInformation("Hit Ratio: {Num5} < {Hit}", num5, hitRatio);

        double range;

        if (num5 <= hitRatio)
        {
            logger.LogInformation("Weapon Hit");

            result.hit = true;

            var talentGrain = orleans.GetTalentGrain(playerId);

            range = ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties, weaponUnit,
                WeaponUnit.d_baseDamage) * ammoDef.DamageModifier;
            range = EffectSystem.ApplyModifiers(range,
                EffectSystem.RegroupModifiers(await talentGrain.Bonuses(weaponInfo.elementType))
                    .GetValueOrDefault("damageBuff"));
            range = EffectSystem.ApplyModifiers(range,
                EffectSystem.RegroupModifiers(await talentGrain.Bonuses(ammoTypeId))
                    .GetValueOrDefault("damageModifier"));
            var shieldHitResult = await targetConstructFightGrain.ConstructTakeHit(new WeaponShotPower()
            {
                ammoType = ammoTypeId,
                power = range,
                originPlayerId = playerId,
                originConstructId = weaponFire.targetId
            });

            if (shieldHitResult.effect == ShieldHitEffect.ShieldAbsorbedHit)
            {
                result.shieldDamage = shieldHitResult.shieldDamage;
                result.rawShieldDamage = shieldHitResult.rawShieldDamage;
                result.coreUnitStressDamage = shieldHitResult.coreUnitStressDamage;

                logger.LogInformation("Shield Absorbed Hit");
            }
            else
            {
                logger.LogInformation("Shield NOT Absorbed Hit");

                var constructInfoGrain = orleans.GetConstructInfoGrain(weaponFire.constructId);
                var constructInfo = await constructInfoGrain.Get();
                var deathInfoPvp = new PlayerDeathInfoPvPData
                {
                    weaponId = weaponFire.weaponId,
                    weaponTypeId = weaponInfo.elementType,
                    constructId = constructId,
                    constructName = constructInfo.rData.name,
                    playerId = playerId,
                    playerName = playerName,
                    ownerId = constructOwner
                };
                result.coreUnitStressDamage = shieldHitResult.coreUnitStressDamage;
                result.coreUnitDestroyed = shieldHitResult.coreUnitStressDestroyed;

                var targetConstructDamageElementGrain = orleans.GetConstructDamageElementsGrain(weaponFire.targetId);

                if (shieldHitResult.coreUnitStressDestroyed)
                {
                    logger.LogInformation("Core Unit Destroyed");

                    await targetConstructDamageElementGrain.TriggerCoreUnitStressDestruction(deathInfoPvp);
                }

                var playerListAndPosition = await targetConstructGrain.GetKillablePlayerListAndPosition();
                var voxelResult =
                    await directServiceGrain.MakeVoxelDamages(weaponFire, ammoDef, range, playerListAndPosition);
                var deathInfo = new PlayerDeathInfo
                {
                    reason = DeathReason.WeaponShot,
                    pvpData = deathInfoPvp
                };

                if (voxelResult.damageOutput != null)
                {
                    logger.LogInformation("Damage Output != NULL");

                    var damageResult =
                        await targetConstructDamageElementGrain.ApplyPvpElementsDamage(
                            voxelResult.damageOutput.elements, deathInfoPvp);
                    result.totalDamage = voxelResult.damageOutput.totalDamage;

                    foreach (var player in voxelResult.damageOutput.deadPlayers)
                    {
                        var deadPlayerGrain = orleans.GetPlayerGrain(player);

                        await deadPlayerGrain.PlayerDieOperation(deathInfo);
                        var namedEntityList = result.playersKilled;
                        var namedEntity1 = new NamedEntity
                        {
                            id = new EntityId
                            {
                                playerId = player
                            },
                            name = (await deadPlayerGrain.GetPlayerInfo()).name
                        };

                        namedEntityList.Add(namedEntity1);
                    }

                    result.coreUnitDestroyed |= damageResult.CoreUnitDestroyed;
                    result.destroyedElementTypes = damageResult.broken
                        .Select(t => t.Item2)
                        .ToList();
                }
            }
        }
        else
        {
            logger.LogInformation("Weapon Miss");

            result.hit = false;
            weaponImpact1 = CalculateMissImpact(
                random,
                weaponWorldLocation.position,
                bboxCenterWorld.position,
                ((Vector3D)weaponFire.bboxSizeLocal).Length * 0.5,
                num5 - hitRatio
            );
        }

        if (result.hit)
        {
            weaponImpact1.ImpactPositionLocal = weaponFire.impactPoint;
            weaponImpact1.TargetId = weaponFire.targetId;
            weaponImpact1.ImpactElementId = weaponFire.impactElementId;
            weaponImpact1.ImpactElementType = weaponFire.impactElementType;
            weaponImpact1.ImpactVoxelMaterialId = weaponFire.impactVoxelMaterialId;
        }

        var shot = new WeaponShot
        {
            id = (ulong)TimePoint.Now().networkTime,
            originConstructId = constructId,
            weaponId = weaponFire.weaponId,
            weaponType = weaponInfo.elementType,
            ammoType = ammoTypeId,
            originPositionLocal = weaponInfo.position,
            originPositionWorld = weaponWorldLocation.position,
            targetConstructId = weaponImpact1.TargetId,
            impactPositionLocal = weaponImpact1.ImpactPositionLocal,
            impactPositionWorld = weaponImpact1.ImpactPositionWorld,
            impactVoxelMaterialId = weaponImpact1.ImpactVoxelMaterialId,
            impactElementId = weaponImpact1.ImpactElementId,
            impactElementType = weaponImpact1.ImpactElementType,
            shieldDamage = result.shieldDamage,
            rawShieldDamage = result.rawShieldDamage,
            coreUnitDestroyed = result.coreUnitDestroyed
        };

        logger.LogInformation("Propagate Shot Impact");
        await directServiceGrain.PropagateShotImpact(shot);

        return result;
    }

    private class WeaponImpact
    {
        public Vec3 ImpactPositionLocal { get; set; }

        public Vec3 ImpactPositionWorld { get; set; }

        public ulong ImpactVoxelMaterialId { get; set; }

        public ulong ImpactElementId { get; set; }

        public ulong ImpactElementType { get; set; }

        public ulong TargetId { get; set; }
    }

    private WeaponImpact CalculateMissImpact(
        Random random,
        Vec3 origin,
        Vec3 target,
        double size,
        double missRange)
    {
        var vector3D1 = (Vector3D)(target - origin);
        var orthogonal = vector3D1.Orthogonal;
        var unitVector3D1 = vector3D1.CrossProduct(orthogonal).Normalize();
        var num = random.NextDouble() * (2.0 * Math.PI);
        var unitVector3D2 = (Math.Cos(num) * orthogonal + Math.Sin(num) * unitVector3D1).Normalize();
        var vector3D2 = size * (0.5 + random.NextDouble() * missRange) * unitVector3D2;
        var vector3D3 = size * (0.5 + random.NextDouble() * missRange) * vector3D1.Normalize();

        return new WeaponImpact
        {
            ImpactPositionWorld = (Vector3D)origin + vector3D1 + vector3D3 + vector3D2
        };
    }

    private static Line3D ComputeWeaponDirection(Ray weaponRay, ElementInfo weaponInfo)
    {
        return new Line3D((Point3D)weaponInfo.position + ((Quaternion)weaponInfo.rotation).Rotate(weaponRay.start),
            (Point3D)weaponInfo.position + ((Quaternion)weaponInfo.rotation).Rotate(weaponRay.end));
    }

    private static async Task<(double, Angle)> CalculateDistance(
        IScenegraph sceneGraph,
        Ray weaponRay,
        WeaponFire weaponFire,
        ElementInfo weaponInfo)
    {
        var weaponRayLocal = ComputeWeaponDirection(weaponRay, weaponInfo);
        var loc = new RelativeLocation
        {
            constructId = weaponFire.targetId,
            position = weaponFire.impactPoint
        };

        ConstructId constructId = weaponFire.constructId;
        var relativeLocation = await sceneGraph.ResolveRelativeLocation(loc, constructId);
        var angle = weaponRayLocal.Direction.AngleTo(
            (Vector3D)relativeLocation.position - weaponRayLocal.StartPoint.ToVector3D()
        );

        return (weaponRayLocal.StartPoint.DistanceTo(relativeLocation.position), angle);
    }

    private static async Task<double> CalculateHitRatio(
        IServiceProvider provider,
        PlayerId playerId,
        IClusterClient orleans,
        Ray weaponRay,
        WeaponUnit weaponUnit,
        WeaponFire weaponFire,
        ElementInfo weaponInfo,
        Ammo ammoDef,
        RelativeLocation weaponWorldTransform,
        RelativeLocation impactWorldTransform)
    {
        var sceneGraph = provider.GetRequiredService<IScenegraph>();

        var (num1, angle) = await CalculateDistance(sceneGraph, weaponRay, weaponFire, weaponInfo);

        var talentGrain = orleans.GetTalentGrain(playerId);

        var weaponModifiers =
            EffectSystem.RegroupModifiers(await talentGrain.Bonuses(weaponInfo.elementType));
        var ammoModifiers =
            EffectSystem.RegroupModifiers(await talentGrain.Bonuses(ammoDef.Definition().Id));
        var accuracy = weaponUnit.BaseAccuracy * ammoDef.AccuracyModifier;
        var valueOrDefault1 =
            weaponModifiers.GetValueOrDefault("optimalAimingConeBuff");
        var angleOptimalValue =
            EffectSystem.ApplyModifiers(
                ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties,
                    weaponUnit, WeaponUnit.d_baseOptimalAimingCone), valueOrDefault1) *
            ammoDef.AimingConeModifier;
        angleOptimalValue = EffectSystem.ApplyModifiers(angleOptimalValue,
            ammoModifiers.GetValueOrDefault("aimingConeModifier"));
        var angleFallOffValue = weaponUnit.FalloffAimingCone * ammoDef.AimingConeModifier;
        var angleFactor = ComputeFactor(angle.Degrees, angleOptimalValue, angleFallOffValue);
        var valueOrDefault2 =
            weaponModifiers.GetValueOrDefault("optimalDistanceBuff");
        var distanceOptimalValue =
            EffectSystem.ApplyModifiers(
                ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties,
                    weaponUnit, WeaponUnit.d_baseOptimalDistance), valueOrDefault2) *
            ammoDef.OptimalDistanceModifier;
        distanceOptimalValue = EffectSystem.ApplyModifiers(distanceOptimalValue,
            ammoModifiers.GetValueOrDefault("optimalDistanceModifier"));
        var distanceFallOffValue = weaponUnit.FalloffDistance * ammoDef.FalloffDistanceModifier;
        var distanceFactor = ComputeFactor(num1, distanceOptimalValue, distanceFallOffValue);
        var toLocalWeaponRotation = ((Quaternion)weaponWorldTransform.rotation).Inversed;
        var localTargetPosition =
            toLocalWeaponRotation.Rotate(impactWorldTransform.position - weaponWorldTransform.position);

        var constructGrain = orleans.GetConstructGrain(weaponFire.constructId);
        var targetConstructGrain = orleans.GetConstructGrain(weaponFire.targetId);

        var (vec3, v) = await constructGrain.GetConstructVelocity();
        var degrees = ComputeAngularVelocity(localTargetPosition, toLocalWeaponRotation.Rotate(
                (await targetConstructGrain.GetConstructVelocity()).Item1 - vec3),
            toLocalWeaponRotation.Rotate(v)
        ).Degrees;

        var valueOrDefault3 =
            weaponModifiers.GetValueOrDefault("optimalTrackingBuff");
        var optimalValue = EffectSystem.ApplyModifiers(
            EffectSystem.ApplyModifiers(
                ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties,
                    weaponUnit, WeaponUnit.d_baseOptimalTracking), valueOrDefault3) *
            ammoDef.TrackingModifier, ammoModifiers.GetValueOrDefault("trackingModifier"));
        var falloffValue = weaponUnit.FalloffTracking * ammoDef.TrackingModifier;
        var factor = ComputeFactor(degrees, optimalValue, falloffValue);
        var num2 = weaponUnit.OptimalCrossSectionDiameter * 0.5;
        var num3 = Math.Min(1.0,
            Math.Sqrt(weaponFire.crossSection / (num2 * num2 * Math.PI)) * (1.0 - factor) + factor);

        var hitRatio = accuracy * num3 * angleFactor * distanceFactor * factor;

        return hitRatio;
    }

    private static async Task StartCycleTimer(
        IClusterClient orleans,
        ulong constructId,
        PlayerId playerId,
        ElementInfo weaponInfo,
        WeaponUnit weaponUnit
    )
    {
        var talentGrain = orleans.GetTalentGrain(playerId);

        if (TimerCache.TryGetValue(new WeaponShotTimerKey(constructId, weaponInfo.elementId), out _))
            throw new BusinessException(ErrorCode.WeaponNotReady, "Weapon is on cooldown");

        var dictionary =
            EffectSystem.RegroupModifiers(await talentGrain.Bonuses(weaponInfo.elementType));

        var num1 = EffectSystem.ApplyModifiers(weaponUnit.CycleTimeBuff,
            dictionary.GetValueOrDefault("cycleTimeBuff"));
        var propertyOrDefault =
            ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties, weaponUnit, WeaponUnit.d_baseCycleTime);
        var cycleTimeWithTalents = propertyOrDefault * num1;

        // NQRegisterTimer(
        //     constructId,
        //     "ResetReadyTimer",
        //     () => "ResetReadyTimer",
        //     (object)null, TimeSpan.FromSeconds(cycleTimeWithTalents),
        //     TimeSpan.FromSeconds(1.0)
        // );

        RegisterShotTimer(constructId, weaponInfo.elementId, cycleTimeWithTalents);
    }

    private static void RegisterShotTimer(
        ulong constructId,
        ulong weaponElementId,
        double cycleTimeWithTalents
    )
    {
        var timerKey = new WeaponShotTimerKey(constructId, weaponElementId);

        TimerCache.Set(timerKey, true, TimeSpan.FromSeconds(cycleTimeWithTalents));
    }

    public struct WeaponShotTimerKey(ulong constructId, ulong elementId)
    {
        public ulong ConstructId { get; } = constructId;
        public ulong ElementId { get; } = elementId;

        public override string ToString() => $"{ConstructId}-{ElementId}";
    }

    private static async Task CheckSeat(
        IServiceProvider provider,
        IClusterClient orleans,
        WeaponFire weaponFire,
        PlayerId playerId,
        ElementId seatId
    )
    {
        var constructElementsGrain = orleans.GetConstructElementsGrain(weaponFire.constructId);
        var bank = provider.GetRequiredService<IGameplayBank>();

        var element = await constructElementsGrain.GetElement(seatId);
        switch (bank.GetBaseObject<Element>(element.elementType))
        {
            case PVPSeatUnit _:
            case KinematicsController _:
                var flag = false;
                foreach (var link in element.links)
                {
                    if (link.plugType == PlugType.PLUG_CONTROL && (ElementId)link.fromElementId == seatId &&
                        (ElementId)link.toElementId == weaponFire.weaponId)
                        flag = true;
                }

                if (!flag)
                    throw new BusinessException(ErrorCode.LinkSourceInvalid, "weapon not linked to container");
                var elementUser = await constructElementsGrain.GetElementUser(seatId);

                if ((elementUser.HasValue
                        ? (elementUser.HasValue ? (elementUser.GetValueOrDefault() != playerId ? 1 : 0) : 0)
                        : 1) != 0)
                    throw new BusinessException(ErrorCode.ElementNotUsed, "attacker need to use a seat");
                break;
            default:
                throw new BusinessException(ErrorCode.ElementInvalidType,
                    "trying to get ammo from a not ammo container");
        }
    }

    private static Task SetDynamicProperty<T>(
        IServiceProvider provider,
        WeaponFire weaponFire,
        IDynamicProperty<T> prop,
        T value
    )
    {
        var dataAccessor = provider.GetRequiredService<IDataAccessor>();

        return dataAccessor.SetDynamicProperty(weaponFire.constructId, weaponFire.weaponId, prop, value);
    }

    private static Angle ComputeAngularVelocity(
        Vector3D target,
        Vector3D relativeVelocity,
        Vector3D localAngularVelocity)
    {
        return Angle.FromRadians((target.CrossProduct(relativeVelocity) / (target.Length * target.Length) -
                                  localAngularVelocity).Length);
    }

    private static double ComputeFactor(
        double value,
        double optimalValue,
        double falloffValue,
        double factorValue = 1.0)
    {
        return Math.Pow(0.5, factorValue * Math.Pow(Math.Max(0.0, value - optimalValue) / falloffValue, 2.0));
    }
}