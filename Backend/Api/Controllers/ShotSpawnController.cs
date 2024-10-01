using System;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Spawner.Behaviors;
using Mod.DynamicEncounters.Features.Spawner.Data;
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
        long shooterConstructId,
        long targetConstructId,
        [FromBody] ShotRequest request
    )
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();
        var npcShotGrain = orleans.GetNpcShotGrain();
        var targetConstructInfoGrain = orleans.GetConstructInfoGrain((ulong)targetConstructId);
        var targetConstructInfo = await targetConstructInfoGrain.Get();
        var bank = provider.GetGameplayBank();

        var targetConstructElementsGrain = orleans.GetConstructElementsGrain((ulong)targetConstructId);
        var elements = await targetConstructElementsGrain.GetElementsOfType<ConstructElement>();

        var targetPos = targetConstructInfo.rData.position;

        var constructElementsGrain = orleans.GetConstructElementsGrain((ulong)shooterConstructId);

        var weaponsElements = await constructElementsGrain.GetElementsOfType<WeaponUnit>();
        var elementInfos = await Task.WhenAll(
            weaponsElements.Select(constructElementsGrain.GetElement)
        );

        var weaponUnits = elementInfos
            .Select(ei => new AggressiveBehavior.WeaponHandle(ei, bank.GetBaseObject<WeaponUnit>(ei)!))
            .Where(w => w.Unit is not StasisWeaponUnit) // TODO Implement Stasis later
            .ToList();

        for (var i = 0; i < request.Iterations; i++)
        {
            var random = provider.GetRequiredService<IRandomProvider>().GetRandom();
            var randomDirection = random.RandomDirectionVec3() * 30000;

            var shootPos = randomDirection + targetPos;
            
            var weapon = random.PickOneAtRandom(weaponUnits);
            var w = weapon.Unit;

            var weaponMod = request.WeaponModifiers;
            var targetElement = random.PickOneAtRandom(elements);
            var targetElementInfo = await targetConstructElementsGrain.GetElement(targetElement.elementId);

            await npcShotGrain.Fire(
                "Random",
                shootPos,
                (ulong)shooterConstructId,
                (ulong)targetConstructInfo.rData.geometry.size,
                (ulong)targetConstructId,
                targetPos,
                new SentinelWeapon
                {
                    aoe = true,
                    damage = w.baseDamage * weaponMod.Damage,
                    range = 400000,
                    aoeRange = 100000,
                    baseAccuracy = w.baseAccuracy * weaponMod.Accuracy,
                    effectDuration = 1,
                    effectStrength = 1,
                    falloffDistance = w.falloffDistance * weaponMod.FalloffDistance,
                    falloffTracking = w.falloffTracking * weaponMod.FalloffTracking,
                    fireCooldown = 1,
                    baseOptimalDistance = w.baseOptimalDistance * weaponMod.OptimalDistance,
                    falloffAimingCone = w.falloffAimingCone * weaponMod.FalloffAimingCone,
                    baseOptimalTracking = w.baseOptimalTracking * weaponMod.OptimalTracking,
                    baseOptimalAimingCone = w.baseOptimalAimingCone * weaponMod.OptimalAimingCone,
                    optimalCrossSectionDiameter = w.optimalCrossSectionDiameter,
                    ammoItem = request.AmmoItem,
                    weaponItem = request.WeaponItem
                },
                5,
                targetElementInfo.position
            );

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
                until = (DateTime.Now + request.DurationSpan).ToNQTimePoint(),
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
        public double Wait { get; set; } = 500;
        public int Iterations { get; set; } = 10;
        public string AmmoItem { get; set; } = "AmmoCannonSmallKineticAdvancedPrecision";
        public string WeaponItem { get; set; } = "WeaponCannonSmallPrecision5";
        public BehaviorModifiers.WeaponModifiers WeaponModifiers { get; set; } = new();
    }
}