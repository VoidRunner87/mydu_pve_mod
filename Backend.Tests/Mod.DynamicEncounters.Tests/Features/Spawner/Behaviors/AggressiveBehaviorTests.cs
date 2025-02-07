using Backend.Scenegraph;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Common.Services;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.VoxelService.Interfaces;
using Mod.DynamicEncounters.Tests.Stubs.Weapons;
using NQ;
using NQ.Interfaces;
using NSubstitute;
using Orleans;

namespace Mod.DynamicEncounters.Tests.Features.Spawner.Behaviors;

[TestFixture]
public class AggressiveBehaviorTests
{
    [Test]
    public void Should_Verify_Rate_Of_Fire()
    {
        var randomProvider = Substitute.For<IRandomProvider>();
        randomProvider.GetRandom().Returns(new Random(1234));

        var npcShotGrain = Substitute.For<INpcShotGrain>();

        var orleans = Substitute.For<IClusterClient>();
        orleans.GetNpcShotGrain().Returns(npcShotGrain);

        var constructElementsService = Substitute.For<IConstructElementsService>();
        constructElementsService.GetDamagingWeaponsEffectiveness(Arg.Is<ulong>(1U))
            .Returns(new Dictionary<string, List<WeaponEffectivenessData>>
            {
                {"WeaponRailgunLargeDefense4", [
                    new WeaponEffectivenessData
                    {
                        Name = "WeaponRailgunLargeDefense4",
                        HitPointsRatio = 1d
                    },
                    new WeaponEffectivenessData
                    {
                        Name = "WeaponRailgunLargeDefense4",
                        HitPointsRatio = 0d
                    }
                ]},
                {"WeaponCannonMediumDefense4", [
                    new WeaponEffectivenessData
                    {
                        Name = "WeaponCannonMediumDefense4",
                        HitPointsRatio = 1d
                    },
                    new WeaponEffectivenessData
                    {
                        Name = "WeaponCannonMediumDefense4",
                        HitPointsRatio = 1d
                    }
                ]}
            });
        
        var constructService = Substitute.For<IConstructService>();
        constructService.GetConstructInfoAsync(Arg.Is<ulong>(1U))
            .Returns(new ConstructInfoOutcome(true, new ConstructInfo
            {
                rData = new ConstructRelativeData
                {
                    position = new Vec3 { x = 100000 },
                    geometry = new ConstructGeometry
                    {
                        size = 256
                    }
                }
            }));
        constructService.GetConstructInfoAsync(Arg.Is<ulong>(2U))
            .Returns(new ConstructInfoOutcome(true, new ConstructInfo
            {
                rData = new ConstructRelativeData
                {
                    position = new Vec3 { z = 100000 },
                    geometry = new ConstructGeometry
                    {
                        size = 256
                    }
                }
            }));

        var voxelServiceClient = Substitute.For<IVoxelServiceClient>();
        var sceneGraph = Substitute.For<IScenegraph>();

        var services = new ServiceCollection();
        services.AddSingleton(randomProvider);
        services.AddSingleton(orleans);
        services.AddSingleton(constructElementsService);
        services.AddSingleton(constructService);
        services.AddSingleton(voxelServiceClient);
        services.AddSingleton(sceneGraph);
        services.AddSingleton<IDateTimeProvider>(new DefaultDateTimeProvider());
        services.AddLogging();

        var provider = services.BuildServiceProvider();

        var prefabItem = new PrefabItem
        {
            Mods = new BehaviorModifiers
            {
                Velocity = new BehaviorModifiers.VelocityModifiers(),
                Weapon = new BehaviorModifiers.WeaponModifiers
                {
                    CycleTime = (float)WeaponItem.FullBuff,
                },
            },
        };

        var prefab = new Prefab(prefabItem);
        var behavior = new AggressiveBehavior(1, prefab);
        var context = new BehaviorContext(1, 1, null, new Vec3(), provider, prefab)
        {
            Provider = provider,
            DamageData = new ConstructDamageData([
                WeaponItemStubFactory.RareLargeDefenseRailgun(),
                WeaponItemStubFactory.RareMediumDefenseCannon()
            ])
        };
        context.SetTargetConstructId(2);

        context.SetPosition(new Vec3 { z = 200000 });

        Assert.DoesNotThrowAsync(async () =>
        {
            await behavior.InitializeAsync(context);
            await behavior.TickAsync(context);
        });
        
        Assert.That(context.ShotWaitTime, Is.GreaterThan(18.14));
        Assert.That(context.ShotWaitTime, Is.LessThanOrEqualTo(18.16));
    }
    
    [Test]
    public void Should_Output_Rare_Railgun_Defense_Large_Weapon_Stats()
    {
        var weaponItem = WeaponItemStubFactory.RareLargeDefenseRailgun();
        var ammoItems = weaponItem.GetAmmoItems();
        var ammo = ammoItems.First();

        const int weaponCount = 2;
        
        var shotWaitTimePerGun = weaponItem.GetShotWaitTimePerGun(ammo, weaponCount);
        var shotWaitTime = weaponItem.GetShotWaitTime(ammo);
        var sustainedRateOfFire = weaponItem.GetSustainedRateOfFire(ammo);
        var numberOfShotsInMagazine = weaponItem.GetNumberOfShotsInMagazine(ammo);
        var totalCycleTime = weaponItem.GetTotalCycleTime(ammo);
        var timeToEmpty = weaponItem.GetTimeToEmpty(ammo);
        var reloadTime = weaponItem.GetReloadTime();
        var shotsPerMinute = 60d / shotWaitTimePerGun;
        
        Console.WriteLine($"{weaponItem.ItemTypeName}:");
        Console.WriteLine($"{nameof(shotWaitTimePerGun)} = {weaponCount}x {shotWaitTimePerGun}");
        Console.WriteLine($"{nameof(shotWaitTime)} = {shotWaitTime}");
        Console.WriteLine($"{nameof(shotsPerMinute)} = {shotsPerMinute}");
        Console.WriteLine($"{nameof(sustainedRateOfFire)} = {sustainedRateOfFire}");
        Console.WriteLine($"{nameof(numberOfShotsInMagazine)} = {numberOfShotsInMagazine}");
        Console.WriteLine($"{nameof(totalCycleTime)} = {totalCycleTime}");
        Console.WriteLine($"{nameof(timeToEmpty)} = {timeToEmpty}");
        Console.WriteLine($"{nameof(reloadTime)} = {reloadTime}");
        
        Assert.That(shotsPerMinute, Is.EqualTo(6.611747836202212));
    }
    
    [Test]
    public void Should_Output_Rare_Cannon_Defense_Medium_Weapon_Stats()
    {
        var weaponItem = WeaponItemStubFactory.RareMediumDefenseCannon();
        var ammoItems = weaponItem.GetAmmoItems();
        var ammo = ammoItems.First();

        const int weaponCount = 2;
        
        var shotWaitTimePerGun = weaponItem.GetShotWaitTimePerGun(ammo, weaponCount);
        var shotWaitTime = weaponItem.GetShotWaitTime(ammo);
        var sustainedRateOfFire = weaponItem.GetSustainedRateOfFire(ammo);
        var numberOfShotsInMagazine = weaponItem.GetNumberOfShotsInMagazine(ammo);
        var totalCycleTime = weaponItem.GetTotalCycleTime(ammo);
        var timeToEmpty = weaponItem.GetTimeToEmpty(ammo);
        var reloadTime = weaponItem.GetReloadTime();
        var shotsPerMinute = 60d / shotWaitTimePerGun;
        
        Console.WriteLine($"{weaponItem.ItemTypeName}:");
        Console.WriteLine($"{nameof(shotWaitTimePerGun)} = {weaponCount}x {shotWaitTimePerGun}");
        Console.WriteLine($"{nameof(shotsPerMinute)} = {shotsPerMinute}");
        Console.WriteLine($"{nameof(shotWaitTime)} = {shotWaitTime}");
        Console.WriteLine($"{nameof(sustainedRateOfFire)} = {sustainedRateOfFire}");
        Console.WriteLine($"{nameof(numberOfShotsInMagazine)} = {numberOfShotsInMagazine}");
        Console.WriteLine($"{nameof(totalCycleTime)} = {totalCycleTime}");
        Console.WriteLine($"{nameof(timeToEmpty)} = {timeToEmpty}");
        Console.WriteLine($"{nameof(reloadTime)} = {reloadTime}");
        
        Assert.That(shotsPerMinute, Is.EqualTo(19.459612183027005));
    }
}