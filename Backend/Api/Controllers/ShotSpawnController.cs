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
using NQ.Interfaces;
using NQutils.Def;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("shot")]
public class ShotSpawnController : Controller
{
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
        var constructInfoGrain = orleans.GetConstructInfoGrain((ulong)targetConstructId);
        var constructInfo = await constructInfoGrain.Get();
        var bank = provider.GetGameplayBank();

        var targetConstructElementsGrain = orleans.GetConstructElementsGrain((ulong)targetConstructId);
        var elements = await targetConstructElementsGrain.GetElementsOfType<ConstructElement>();

        var targetPos = constructInfo.rData.position;

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
                (ulong)constructInfo.rData.geometry.size,
                (ulong)targetConstructId,
                targetPos,
                new SentinelWeapon
                {
                    aoe = true,
                    damage = w.baseDamage * weaponMod.Damage,
                    range = 400000,
                    aoeRange = 100000,
                    baseAccuracy = w.baseAccuracy * weaponMod.Accuracy,
                    effectDuration = 10,
                    effectStrength = 10,
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

    public class ShotRequest
    {
        public double Wait { get; set; } = 500;
        public int Iterations { get; set; } = 10;
        public string AmmoItem { get; set; } = "AmmoCannonSmallKineticAdvancedPrecision";
        public string WeaponItem { get; set; } = "WeaponCannonSmallPrecision5";
        public BehaviorModifiers.WeaponModifiers WeaponModifiers { get; set; } = new();
    }
}