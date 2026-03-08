namespace NpcWeaponLib.Data;

/// <summary>
/// Complete weapon statistics and fire rate calculations. Pure data with pure methods.
/// </summary>
/// <remarks>
/// <para>
/// Ported from <c>Mod.DynamicEncounters.Features.Common.Data.WeaponItem</c>.
/// The original wraps <c>NQutils.Def.WeaponUnit</c>; this version is standalone.
/// </para>
/// <para>
/// Fire rate pipeline:
/// <list type="number">
///   <item><see cref="GetNumberOfShotsInMagazine"/> — magazine volume / ammo volume</item>
///   <item><see cref="GetTimeToEmpty"/> — shots * cycle time</item>
///   <item><see cref="GetTotalCycleTime"/> — time to empty + reload time</item>
///   <item><see cref="GetSustainedRateOfFire"/> — shots / total cycle</item>
///   <item><see cref="GetShotWaitTime"/> — 1 / sustained ROF (clamped)</item>
///   <item><see cref="GetShotWaitTimePerGun"/> — wait time / weapon count</item>
/// </list>
/// </para>
/// </remarks>
public class WeaponStats
{
    /// <summary>Default magazine buff factor applied to magazine volume (1.5x).</summary>
    public const double FullMagBuff = 1.5d;

    /// <summary>Default buff factor applied to cycle time and reload time (0.5625x).</summary>
    public const double FullBuff = 0.5625d;

    /// <summary>Unique item type identifier from the game bank.</summary>
    public required ulong ItemTypeId { get; set; }

    /// <summary>Internal item type name (e.g., "WeaponMissileSmallPrecision3").</summary>
    public required string ItemTypeName { get; set; }

    /// <summary>Human-readable weapon name (e.g., "Rare Military Small Railgun s").</summary>
    public required string DisplayName { get; set; }

    /// <summary>Base damage per shot before modifiers.</summary>
    public required double BaseDamage { get; set; }

    /// <summary>Base hit probability at optimal range (0-1).</summary>
    public required double BaseAccuracy { get; set; }

    /// <summary>Optimal engagement distance in metres.</summary>
    public required double BaseOptimalDistance { get; set; }

    /// <summary>Distance beyond optimal where effectiveness degrades, in metres.</summary>
    public required double FalloffDistance { get; set; }

    /// <summary>Tracking effectiveness at optimal range.</summary>
    public required double BaseOptimalTracking { get; set; }

    /// <summary>Tracking degradation beyond optimal range.</summary>
    public required double FalloffTracking { get; set; }

    /// <summary>Aiming cone half-angle at optimal range.</summary>
    public required double BaseOptimalAimingCone { get; set; }

    /// <summary>Aiming cone expansion beyond optimal range.</summary>
    public required double FalloffAimingCone { get; set; }

    /// <summary>Ideal target cross-section diameter in metres.</summary>
    public required double OptimalCrossSectionDiameter { get; set; }

    /// <summary>Time between shots in a single magazine, in seconds.</summary>
    public required double BaseCycleTime { get; set; }

    /// <summary>Time to reload an empty magazine, in seconds.</summary>
    public required double BaseReloadTime { get; set; }

    /// <summary>Magazine capacity in litres (before buff factor).</summary>
    public required double MagazineVolume { get; set; }

    /// <summary>All compatible ammo types for this weapon.</summary>
    public required IReadOnlyList<AmmoStats> AmmoItems { get; set; }

    /// <summary>
    /// Half-falloff firing distance: optimal + falloff/2.
    /// Used by <see cref="WeaponSelector"/> to match weapons to target range.
    /// </summary>
    /// <returns>Distance in metres representing the weapon's effective midpoint range.</returns>
    public double GetHalfFalloffDistance() => BaseOptimalDistance + FalloffDistance / 2;

    /// <summary>Number of shots per magazine given ammo unit volume and magazine buff.</summary>
    /// <param name="ammo">Ammo type whose <see cref="AmmoStats.UnitVolume"/> determines shots per magazine.</param>
    /// <param name="magBuff">Magazine volume multiplier. Defaults to <see cref="FullMagBuff"/> (1.5x).</param>
    /// <returns>Whole number of shots that fit in the magazine.</returns>
    public double GetNumberOfShotsInMagazine(AmmoStats ammo, double magBuff = FullMagBuff)
        => Math.Floor(MagazineVolume * magBuff / ammo.UnitVolume);

    /// <summary>Time to fire all shots in a full magazine, in seconds.</summary>
    /// <param name="ammo">Ammo type for magazine capacity calculation.</param>
    /// <param name="magBuff">Magazine volume multiplier. Defaults to <see cref="FullMagBuff"/>.</param>
    /// <param name="cycleBuff">Cycle time multiplier. Defaults to <see cref="FullBuff"/>.</param>
    /// <returns>Time in seconds to fire the entire magazine.</returns>
    public double GetTimeToEmpty(AmmoStats ammo, double magBuff = FullMagBuff, double cycleBuff = FullBuff)
        => GetNumberOfShotsInMagazine(ammo, magBuff) * (BaseCycleTime * cycleBuff);

    /// <summary>Reload duration in seconds, with buff factor.</summary>
    /// <param name="reloadBuff">Reload time multiplier. Defaults to <see cref="FullBuff"/>.</param>
    /// <returns>Reload time in seconds.</returns>
    public double GetReloadTime(double reloadBuff = FullBuff) => BaseReloadTime * reloadBuff;

    /// <summary>Full cycle: fire all shots + reload, in seconds.</summary>
    /// <param name="ammo">Ammo type for magazine capacity calculation.</param>
    /// <param name="magBuff">Magazine volume multiplier. Defaults to <see cref="FullMagBuff"/>.</param>
    /// <param name="cycleBuff">Cycle time multiplier. Defaults to <see cref="FullBuff"/>.</param>
    /// <param name="reloadBuff">Reload time multiplier. Defaults to <see cref="FullBuff"/>.</param>
    /// <returns>Total seconds for one complete fire-and-reload cycle.</returns>
    public double GetTotalCycleTime(AmmoStats ammo, double magBuff = FullMagBuff, double cycleBuff = FullBuff, double reloadBuff = FullBuff)
        => GetTimeToEmpty(ammo, magBuff, cycleBuff) + GetReloadTime(reloadBuff);

    /// <summary>Average shots per second across full cycle (fire + reload).</summary>
    /// <param name="ammo">Ammo type for magazine capacity calculation.</param>
    /// <param name="magBuff">Magazine volume multiplier. Defaults to <see cref="FullMagBuff"/>.</param>
    /// <param name="cycleBuff">Cycle time multiplier. Defaults to <see cref="FullBuff"/>.</param>
    /// <param name="reloadBuff">Reload time multiplier. Defaults to <see cref="FullBuff"/>.</param>
    /// <returns>Sustained rate of fire in shots per second.</returns>
    public double GetSustainedRateOfFire(AmmoStats ammo, double magBuff = FullMagBuff, double cycleBuff = FullBuff, double reloadBuff = FullBuff)
        => GetNumberOfShotsInMagazine(ammo, magBuff) / GetTotalCycleTime(ammo, magBuff, cycleBuff, reloadBuff);

    /// <summary>
    /// Seconds between shots for a single weapon, accounting for sustained ROF.
    /// Clamped: buff factors to [0.1, 5], result floor at BaseCycleTime if ROF is too high.
    /// </summary>
    /// <param name="ammo">Ammo type for magazine capacity calculation.</param>
    /// <param name="magBuff">Magazine volume multiplier, clamped to [0.1, 5]. Defaults to <see cref="FullMagBuff"/>.</param>
    /// <param name="cycleBuff">Cycle time multiplier, clamped to [0.1, 5]. Defaults to <see cref="FullBuff"/>.</param>
    /// <param name="reloadBuff">Reload time multiplier, clamped to [0.1, 5]. Defaults to <see cref="FullBuff"/>.</param>
    /// <returns>Seconds between shots. Falls back to <see cref="BaseCycleTime"/> (clamped to [0.5, 60]) if result is below 0.5 seconds.</returns>
    public double GetShotWaitTime(AmmoStats ammo, double magBuff = FullMagBuff, double cycleBuff = FullBuff, double reloadBuff = FullBuff)
    {
        cycleBuff = Math.Clamp(cycleBuff, 0.1d, 5d);
        reloadBuff = Math.Clamp(reloadBuff, 0.1d, 5d);
        magBuff = Math.Clamp(magBuff, 0.1d, 5d);

        var result = 1d / GetSustainedRateOfFire(ammo, magBuff, cycleBuff, reloadBuff);
        return result <= 0.5d ? Math.Clamp(BaseCycleTime, 0.5d, 60d) : result;
    }

    /// <summary>
    /// Effective wait time when N guns of this type fire in parallel.
    /// <paramref name="weaponCount"/> is clamped to [1, 10].
    /// </summary>
    /// <param name="ammo">Ammo type for magazine capacity calculation.</param>
    /// <param name="weaponCount">Number of parallel weapons, clamped to [1, 10].</param>
    /// <param name="magBuff">Magazine volume multiplier. Defaults to <see cref="FullMagBuff"/>.</param>
    /// <param name="cycleBuff">Cycle time multiplier. Defaults to <see cref="FullBuff"/>.</param>
    /// <param name="reloadBuff">Reload time multiplier. Defaults to <see cref="FullBuff"/>.</param>
    /// <returns>Seconds between shots when <paramref name="weaponCount"/> weapons fire in parallel.</returns>
    public double GetShotWaitTimePerGun(AmmoStats ammo, int weaponCount, double magBuff = FullMagBuff, double cycleBuff = FullBuff, double reloadBuff = FullBuff)
        => GetShotWaitTime(ammo, magBuff, cycleBuff, reloadBuff) / Math.Clamp(weaponCount, 1d, 10d);
}
