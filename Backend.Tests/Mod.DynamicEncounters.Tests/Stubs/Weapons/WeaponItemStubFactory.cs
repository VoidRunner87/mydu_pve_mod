using Mod.DynamicEncounters.Features.Common.Data;
using NQutils.Def;
using NSubstitute;

namespace Mod.DynamicEncounters.Tests.Stubs.Weapons;

public static class WeaponItemStubFactory
{
    public static WeaponItem RareMediumDefenseCannon()
    {
        var amAmmo = Substitute.For<Ammo>();

        var ammoItem1 = new AmmoItem(
            1958427908,
            "AmmoCannonMediumThermicAdvancedAgile",
            amAmmo
        ) { Level = 3, UnitVolume = 100 };

        var ammoItem2 = new AmmoItem(
            3901365200,
            "AmmoCannonMediumKineticAdvancedAgile",
            amAmmo
        ) { Level = 3, UnitVolume = 100 };

        var weaponUnit = new WeaponUnit();
        var weapon = new WeaponItem(
            2383624965,
            "WeaponCannonMediumDefense4",
            weaponUnit,
            [
                ammoItem1,
                ammoItem2
            ]
        )
        {
            BaseAccuracy = 1,
            BaseCycleTime = 5.5672,
            BaseDamage = 383730,
            BaseOptimalAimingCone = 78.5452,
            BaseOptimalDistance = 17873,
            BaseOptimalTracking = 0.9291,
            BaseReloadTime = 70.1438,
            FalloffAimingCone = 135.0,
            FalloffDistance = 30720,
            FalloffTracking = 2.178,
            MagazineVolume = 881.6711,
        };

        return weapon;
    }
    
    public static WeaponItem RareLargeDefenseRailgun()
    {
        var amAmmo = Substitute.For<Ammo>();

        var ammoItem1 = new AmmoItem(
            994404082,
            "AmmoRailgunLargeAntimatterAdvancedAgile",
            amAmmo
        ) { Level = 3, UnitVolume = 750 };

        var ammoItem2 = new AmmoItem(
            493646316,
            "AmmoRailgunLargeElectromagneticAdvancedAgile",
            amAmmo
        ) { Level = 3, UnitVolume = 750 };

        var weaponUnit = new WeaponUnit();
        var weapon = new WeaponItem(
            3670363952,
            "WeaponRailgunLargeDefense4",
            weaponUnit,
            [
                ammoItem1,
                ammoItem2
            ]
        )
        {
            BaseAccuracy = 1,
            BaseCycleTime = 16.1329,
            BaseDamage = 777630,
            BaseOptimalAimingCone = 25.6574,
            BaseOptimalDistance = 83409,
            BaseOptimalTracking = 0.1547,
            BaseReloadTime = 16.1329,
            FalloffAimingCone = 44.1,
            FalloffDistance = 61440,
            FalloffTracking = 1.1979,
            MagazineVolume = 750,
        };

        return weapon;
    }
}