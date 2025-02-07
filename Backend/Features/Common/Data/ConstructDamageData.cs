using System;
using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Common.Data;

public class ConstructDamageData(IEnumerable<WeaponItem> weapons) : IOutcome
{
    public IEnumerable<WeaponItem> Weapons { get; } = weapons;

    public WeaponItem? GetBestDamagingWeapon() => Weapons.MaxBy(w => w.BaseDamage);
    public WeaponItem? GetBestRangedWeapon() => Weapons.MaxBy(GetHalfFalloffFiringDistance);

    public WeaponItem? GetBestWeaponByTargetDistance(double distance)
    {
        return Weapons.Select(w => new
        {
            Weapon = w,
            Delta = Math.Abs(GetHalfFalloffFiringDistance(w) - distance)
        }).MinBy(x => x.Delta)?.Weapon;
    }

    public double GetHalfFalloffFiringDistance(WeaponItem weaponItem) =>
        weaponItem.BaseOptimalDistance + weaponItem.FalloffDistance / 2;
}