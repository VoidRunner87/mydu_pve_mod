namespace NpcWeaponLib;

using NpcWeaponLib.Data;

/// <summary>
/// Computes the effective fire interval for a weapon given ammo, modifiers, and weapon count.
/// Pure function — wraps <see cref="WeaponStats"/> fire rate methods with modifier application.
/// </summary>
public static class WeaponFireRateCalculator
{
    /// <summary>
    /// Calculates the seconds between shots for the given weapon configuration.
    /// </summary>
    /// <param name="weapon">Weapon base stats.</param>
    /// <param name="ammo">Selected ammo type.</param>
    /// <param name="modifiers">Per-NPC weapon modifiers.</param>
    /// <param name="functionalWeaponCount">Number of functional weapons of this type (clamped to [1,10]).</param>
    /// <param name="maxWeaponCount">Max weapon count from prefab config (caps functionalWeaponCount).</param>
    /// <returns>Seconds between shots. Lower = faster firing.</returns>
    public static double CalculateFireInterval(
        WeaponStats weapon,
        AmmoStats ammo,
        WeaponModifiers modifiers,
        int functionalWeaponCount,
        int maxWeaponCount)
    {
        var clampedCount = Math.Clamp(functionalWeaponCount, 0, maxWeaponCount);
        return weapon.GetShotWaitTimePerGun(ammo, clampedCount, cycleBuff: modifiers.CycleTime);
    }
}
