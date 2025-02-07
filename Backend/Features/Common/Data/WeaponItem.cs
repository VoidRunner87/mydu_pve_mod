using System;
using System.Collections.Generic;
using NQutils.Def;

namespace Mod.DynamicEncounters.Features.Common.Data;

public class WeaponItem(ulong itemTypeId, string itemTypeName, WeaponUnit weaponUnit, IEnumerable<AmmoItem> ammoItems)
{
    public const double FullMagBuff = 1.5d;
    public const double FullBuff = 0.5625D;
    
    public ulong ItemTypeId { get; set; } = itemTypeId;
    public string ItemTypeName { get; set; } = itemTypeName;
    public string DisplayName { get; set; } = weaponUnit.DisplayName;
    public double BaseDamage { get; set; } = weaponUnit.BaseDamage;
    public double BaseAccuracy { get; set; } = weaponUnit.BaseAccuracy;
    public double FalloffDistance { get; set; } = weaponUnit.FalloffDistance;
    public double FalloffTracking { get; set; } = weaponUnit.FalloffTracking;
    public double FalloffAimingCone { get; set; } = weaponUnit.FalloffAimingCone;
    public double BaseOptimalTracking { get; set; } = weaponUnit.BaseOptimalTracking;
    public double BaseOptimalDistance { get; set; } = weaponUnit.BaseOptimalDistance;
    public double BaseOptimalAimingCone { get; set; } = weaponUnit.BaseOptimalAimingCone;
    public double OptimalCrossSectionDiameter { get; set; } = weaponUnit.OptimalCrossSectionDiameter;
    public double BaseCycleTime { get; set; } = weaponUnit.BaseCycleTime;
    public double BaseReloadTime { get; set; } = weaponUnit.BaseReloadTime;
    public double MagazineVolume { get; set; } = weaponUnit.MagazineVolume;

    public double GetNumberOfShotsInMagazine(
        AmmoItem ammoItem,
        double magazineBuffFactor = FullMagBuff
    ) => Math.Floor(MagazineVolume * magazineBuffFactor / ammoItem.UnitVolume);

    public double GetTimeToEmpty(
        AmmoItem ammoItem,
        double magazineBuffFactor = FullMagBuff,
        double cycleTimeBuffFactor = FullBuff
    ) => GetNumberOfShotsInMagazine(ammoItem, magazineBuffFactor) * (BaseCycleTime * cycleTimeBuffFactor);

    public double GetReloadTime(double reloadTimeBuffFactor = FullBuff) => BaseReloadTime * reloadTimeBuffFactor;

    public double GetTotalCycleTime(
        AmmoItem ammoItem,
        double magazineBuffFactor = FullMagBuff,
        double cycleTimeBuffFactor = FullBuff,
        double reloadTimeBuffFactor = FullBuff
    ) => GetTimeToEmpty(ammoItem, magazineBuffFactor, cycleTimeBuffFactor) + GetReloadTime(reloadTimeBuffFactor);

    public double GetSustainedRateOfFire(
        AmmoItem ammoItem,
        double magazineBuffFactor = FullMagBuff,
        double cycleTimeBuffFactor = FullBuff,
        double reloadTimeBuffFactor = FullBuff
    ) => GetNumberOfShotsInMagazine(ammoItem, magazineBuffFactor) /
         GetTotalCycleTime(ammoItem, magazineBuffFactor, cycleTimeBuffFactor, reloadTimeBuffFactor);

    public double GetShotWaitTime(
        AmmoItem ammoItem,
        double magazineBuffFactor = FullMagBuff,
        double cycleTimeBuffFactor = FullBuff,
        double reloadTimeBuffFactor = FullBuff
    )
    {
        cycleTimeBuffFactor = Math.Clamp(cycleTimeBuffFactor, 0.1d, 5d);
        reloadTimeBuffFactor = Math.Clamp(reloadTimeBuffFactor, 0.1d, 5d);
        magazineBuffFactor = Math.Clamp(magazineBuffFactor, 0.1d, 5d);

        var result = 1d / GetSustainedRateOfFire(ammoItem, magazineBuffFactor, cycleTimeBuffFactor, reloadTimeBuffFactor);

        if (result <= 0.5d)
        {
            return Math.Clamp(BaseCycleTime, 0.5d, 60);
        }

        return result;
    }

    public double GetShotWaitTimePerGun(
        AmmoItem ammoItem,
        int weaponCount,
        double magazineBuffFactor = FullMagBuff,
        double cycleTimeBuffFactor = FullBuff,
        double reloadTimeBuffFactor = FullBuff
    )
    {
        return GetShotWaitTime(ammoItem, magazineBuffFactor, cycleTimeBuffFactor, reloadTimeBuffFactor) /
               Math.Clamp(weaponCount, 1d, 10d);
    }

    private IEnumerable<AmmoItem> AmmoItems { get; } = ammoItems;

    public IEnumerable<AmmoItem> GetAmmoItems() => AmmoItems;
}