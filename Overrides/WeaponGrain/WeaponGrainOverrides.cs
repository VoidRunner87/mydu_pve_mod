using System;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Database;
using Backend.Scenegraph;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
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

namespace Mod.DynamicEncounters.Overrides.WeaponGrain;

public class WeaponGrainOverrides(IServiceProvider provider)
{
    public async Task<WeaponFireResult> WeaponFireOnce(
        IIncomingGrainCallContext context,
        PlayerId playerId,
        WeaponFire weaponFire
    )
    {
        // if (playerId != 4)
        // {
        //     await context.Invoke();
        //     return (WeaponFireResult)context.Result;
        // }

        var random = new Random();
        var constructId = weaponFire.constructId;

        var sql = provider.GetRequiredService<ISql>();
        var orleans = provider.GetRequiredService<IClusterClient>();
        var bank = provider.GetRequiredService<IGameplayBank>();

        var targetConstructFightGrain = orleans.GetConstructFightGrain(weaponFire.targetId);

        var weaponGrain = context.Grain.AsReference<IWeaponGrain>();
        var weaponInfo = await sql.GetElement(weaponFire.weaponId, fetchLinks: false);
        var directServiceGrain = orleans.GetDirectServiceGrain();

        await targetConstructFightGrain.RefreshPvpTimer();

        var sceneGraph = provider.GetRequiredService<IScenegraph>();

        IElementWeaponAim weaponAim = new ElementWeaponAim(
            provider.GetRequiredService<ILoggerFactory>()
                .CreateLogger<ElementWeaponAim>(),
            bank
        );

        var weaponRay = weaponAim.GetRay(weaponInfo.elementType);

        var weaponWorldLocation = await sceneGraph.ResolveWorldLocation(new RelativeLocation
        {
            constructId = constructId,
            position = weaponInfo.position,
            rotation = weaponInfo.rotation
        });

        var constructGrain = orleans.GetConstructGrain(constructId);

        var constructOwner = await constructGrain.GetOwner();

        var bboxCenterWorld = await sceneGraph.ResolveWorldLocation(new RelativeLocation
        {
            constructId = weaponFire.targetId,
            position = weaponFire.bboxCenterLocal
        });

        var playerName = (await orleans.GetPlayerGrain(playerId).GetPlayerInfo()).name;

        var result = new WeaponFireResult
        {
            constructId = constructId
        };

        var weaponImpact1 = new WeaponImpact();

        var impactWorldTransform = await sceneGraph.ResolveWorldLocation(new RelativeLocation
        {
            constructId = weaponFire.targetId,
            position = weaponFire.impactPoint
        });

        // var weaponElementGrain = orleans.GetConstructElementsGrain(constructId);
        // var weaponElementInfo = weaponElementGrain.GetElement(weaponFire.weaponId);
        var weaponUnit = bank.GetBaseObject<WeaponUnit>(weaponInfo.elementType);
        if (weaponUnit == null)
        {
            throw new BusinessException(ErrorCode.WeaponShotKillInvalid);
        }

        var ammoTypeId =
            (ulong)ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties, weaponUnit,
                WeaponUnit.d_ammoType);
        var ammoDef = bank.GetBaseObject<Ammo>(ammoTypeId);

        if (ammoDef == null)
        {
            throw new BusinessException(ErrorCode.WeaponShotKillInvalid);
        }

        var hitRatio = await CalculateHitRatio(
            provider, 
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

        if (num5 <= hitRatio)
        {
            result.hit = true;

            // TODO
            var power = ElementPropertiesHelper.GetPropertyOrDefault(weaponInfo.properties, weaponUnit,
                WeaponUnit.d_baseDamage) * ammoDef.DamageModifier;
            power *= 1.6d;

            var shieldHitResult = await orleans.GetConstructFightGrain(weaponFire.targetId)
                .ConstructTakeHit(
                    new WeaponShotPower
                    {
                        ammoType = ammoTypeId,
                        power = power,
                        originPlayerId = playerId,
                        originConstructId = constructId
                    }
                );
            if (shieldHitResult.effect == ShieldHitEffect.ShieldAbsorbedHit)
            {
                result.shieldDamage = shieldHitResult.shieldDamage;
                result.rawShieldDamage = shieldHitResult.rawShieldDamage;
                result.coreUnitStressDamage = shieldHitResult.coreUnitStressDamage;
            }
            else
            {
                var deathInfoPvp = new PlayerDeathInfoPvPData
                {
                    weaponId = weaponFire.weaponId,
                    weaponTypeId = weaponInfo.elementType,
                    constructId = constructId,
                    constructName = "TODO",
                    playerId = playerId,
                    playerName = playerName,
                    ownerId = constructOwner
                };
                result.coreUnitStressDamage = shieldHitResult.coreUnitStressDamage;
                result.coreUnitDestroyed = shieldHitResult.coreUnitStressDestroyed;

                var targetConstructDamageElementGrain = orleans.GetConstructDamageElementsGrain(weaponFire.targetId);

                if (shieldHitResult.coreUnitStressDestroyed)
                {
                    await targetConstructDamageElementGrain.TriggerCoreUnitStressDestruction(deathInfoPvp);
                }

                var targetConstructGrain = orleans.GetConstructGrain(weaponFire.targetId);

                var playerListAndPosition = await targetConstructGrain.GetKillablePlayerListAndPosition();
                var voxelResult =
                    await directServiceGrain.MakeVoxelDamages(weaponFire, ammoDef, power, playerListAndPosition);
                var deathInfo = new PlayerDeathInfo
                {
                    reason = DeathReason.WeaponShot,
                    pvpData = deathInfoPvp
                };

                if (voxelResult.damageOutput != null)
                {
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

    private static Quaternion ToQuaternion(Quat q)
    {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }

    private static Line3D ComputeWeaponDirection(Ray weaponRay, ElementInfo weaponInfo)
    {
        return new Line3D(
            (Point3D)weaponInfo.position +
            ToQuaternion(weaponInfo.rotation)
                .Rotate(weaponRay.start),
            (Point3D)weaponInfo.position +
            ToQuaternion(weaponInfo.rotation)
                .Rotate(weaponRay.end)
        );
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

        var accuracy = weaponUnit.BaseAccuracy * ammoDef.AccuracyModifier * 1.6d;
        var angleOptimalValue = weaponUnit.BaseOptimalAimingCone * 1.6d;
        
        var angleFallOffValue = weaponUnit.FalloffAimingCone * ammoDef.AimingConeModifier * 1.6d;
        
        var angleFactor = ComputeFactor(angle.Degrees, angleOptimalValue, angleFallOffValue);
        
        var distanceOptimalValue = weaponUnit.BaseOptimalDistance * 1.6d;

        var distanceFallOffValue = weaponUnit.FalloffDistance * ammoDef.FalloffDistanceModifier;
        
        var distanceFactor = ComputeFactor(num1, distanceOptimalValue, distanceFallOffValue);
        
        var toLocalWeaponRotation = ToQuaternion(weaponWorldTransform.rotation).Inversed;
        var localTargetPosition =
            toLocalWeaponRotation.Rotate(impactWorldTransform.position - weaponWorldTransform.position);

        var constructGrain = orleans.GetConstructGrain(weaponFire.constructId); 
        var targetConstructGrain = orleans.GetConstructGrain(weaponFire.targetId); 
        var (vec3, v) = await constructGrain.GetConstructVelocity();
        
        var degrees = ComputeAngularVelocity(localTargetPosition,
            toLocalWeaponRotation.Rotate((await targetConstructGrain.GetConstructVelocity()).Item1 - vec3),
            toLocalWeaponRotation.Rotate(v)).Degrees;
        
        var optimalValue = weaponUnit.BaseOptimalTracking * 1.6d;
        
        var falloffValue = weaponUnit.FalloffTracking * ammoDef.TrackingModifier;
        
        var factor = ComputeFactor(degrees, optimalValue, falloffValue);
        
        var num2 = weaponUnit.OptimalCrossSectionDiameter * 0.5;
        var num3 = Math.Min(1.0,
            Math.Sqrt(weaponFire.crossSection / (num2 * num2 * Math.PI)) * (1.0 - factor) + factor);

        var hitRatio = accuracy * num3 * angleFactor * distanceFactor * factor;
        
        return hitRatio;
    }
    
    private static Angle ComputeAngularVelocity(
        Vector3D target,
        Vector3D relativeVelocity,
        Vector3D localAngularVelocity)
    {
        return Angle.FromRadians((target.CrossProduct(relativeVelocity) / (target.Length * target.Length) - localAngularVelocity).Length);
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