using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Scenegraph;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.VoxelService.Data;
using Mod.DynamicEncounters.Features.VoxelService.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("shot")]
public class ShotSpawnController : Controller
{
    [SwaggerOperation("Spawns shots on a construct. Useful to make wrecks")]
    [HttpPut]
    [Route("shooter/{shooterConstructId:long}/target/{targetConstructId:long}")]
    public async Task<IActionResult> Shoot(
        ulong shooterConstructId,
        ulong targetConstructId,
        [FromBody] ShotRequest request
    )
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();
        var sceneGraph = provider.GetRequiredService<IScenegraph>();
        var targetConstructInfoGrain = orleans.GetConstructInfoGrain(targetConstructId);
        var shooterConstructInfoGrain = orleans.GetConstructInfoGrain(shooterConstructId);
       
        var constructElementGrain = orleans.GetConstructElementsGrain(shooterConstructId);
        var weapons = await constructElementGrain.GetElementsOfType<WeaponUnit>();
        var firstWeapon = weapons.First();
        var elementInfo = await constructElementGrain.GetElement(firstWeapon);

        for (var i = 0; i < request.Iterations; i++)
        {
            var targetConstructInfo = await targetConstructInfoGrain.Get();
            var shooterConstructInfo = await shooterConstructInfoGrain.Get();
            var targetPos = targetConstructInfo.rData.position;
            var targetRot = targetConstructInfo.rData.rotation;
            var shooterPos = shooterConstructInfo.rData.position;
            var shooterRot = shooterConstructInfo.rData.rotation;
            
            var shooterWeaponLocalPos = elementInfo.position;
            var shooterWeaponPos = VectorMathHelper.CalculateWorldPosition(
                shooterWeaponLocalPos.ToVector3(),
                shooterPos.ToVector3(),
                shooterRot.ToQuat()
            );
            
            var relativePosition = VectorMathHelper.CalculateRelativePosition(
                shooterWeaponPos,
                targetPos.ToVector3(),
                targetRot.ToQuat()
            );
            
            var voxelServiceClient = provider.GetRequiredService<IVoxelServiceClient>();
            var outcome = await voxelServiceClient.QueryRandomPoint(
                new QueryRandomPoint
                {
                    ConstructId = targetConstructId,
                    FromLocalPosition = relativePosition.ToNqVec3()
                }
            );

            if (!outcome.Success)
            {
                return BadRequest(outcome.Message);
            }

            var point = outcome.LocalPosition;
            
            var bank = provider.GetGameplayBank();
            
            var ds = orleans.GetDirectServiceGrain();
            var pos = await sceneGraph.ResolveWorldLocation(new RelativeLocation
            {
                constructId = targetConstructId,
                position = point
            });
            await ds.PropagateShotImpact(new WeaponShot
            {
                id = (ulong)TimePoint.Now().networkTime,
                originConstructId = shooterConstructId,
                originPositionWorld = shooterWeaponPos.ToNqVec3(),
                originPositionLocal = shooterWeaponLocalPos,
                targetConstructId = targetConstructId,
                ammoType = bank.GetDefinition(request.AmmoItem)!.Id,
                weaponType = bank.GetDefinition(request.WeaponItem)!.Id,
                impactPositionWorld = pos.position,
                impactPositionLocal = point,
                impactElementType = 3,
                coreUnitDestroyed = false
            });

            // await npcShotGrain.Fire(
            //     "Random",
            //     shooterWeaponPos.ToNqVec3(),
            //     shooterConstructId,
            //     (ulong)shooterConstructInfo.rData.geometry.size,
            //     targetConstructId,
            //     targetPos,
            //     new SentinelWeapon
            //     {
            //         aoe = true,
            //         damage = w.BaseDamage * weaponMod.Damage,
            //         range = 0,
            //         aoeRange = 1,
            //         baseAccuracy = w.BaseAccuracy * weaponMod.Accuracy,
            //         effectDuration = 1,
            //         effectStrength = 1,
            //         falloffDistance = w.FalloffDistance * weaponMod.FalloffDistance,
            //         falloffTracking = w.FalloffTracking * weaponMod.FalloffTracking,
            //         fireCooldown = 1,
            //         baseOptimalDistance = w.BaseOptimalDistance * weaponMod.OptimalDistance,
            //         falloffAimingCone = w.FalloffAimingCone * weaponMod.FalloffAimingCone,
            //         baseOptimalTracking = w.BaseOptimalTracking * weaponMod.OptimalTracking,
            //         baseOptimalAimingCone = w.BaseOptimalAimingCone * weaponMod.OptimalAimingCone,
            //         optimalCrossSectionDiameter = w.OptimalCrossSectionDiameter,
            //         ammoItem = request.AmmoItem,
            //         weaponItem = request.WeaponItem
            //     },
            //     5,
            //     point
            // );

            await Task.Delay((int)Math.Clamp(request.Wait, 1, 1000));
        }

        return Ok();
    }

    [Route("stasis/{constructId:long}")]
    [HttpPost]
    public async Task<IActionResult> ApplyStasisToConstruct(long constructId, [FromBody] StasisRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var constructInfoGrain = orleans.GetConstructInfoGrain((ulong)constructId);
        await constructInfoGrain.Update(new ConstructInfoUpdate
        {
            additionalMaxSpeedDebuf = new MaxSpeedDebuf
            {
                until = (DateTime.UtcNow + request.DurationSpan).ToNQTimePoint(),
                value = Math.Clamp(request.Value, 0d, 1d)
            }
        });

        return Ok();
    }

    public class StasisRequest
    {
        public TimeSpan DurationSpan { get; set; }
        public double Value { get; set; }
    }

    public class ShotRequest
    {
        public double Wait { get; set; } = 200;
        public int Iterations { get; set; } = 10;
        public string AmmoItem { get; set; } = "AmmoCannonSmallKineticAdvancedPrecision";
        public string WeaponItem { get; set; } = "WeaponCannonSmallPrecision5";
        public BehaviorModifiers.WeaponModifiers WeaponModifiers { get; set; } = new();
    }
}