namespace NpcWeaponLib;

using NpcWeaponLib.Data;

/// <summary>
/// Main entry point for NPC weapon firing simulation. Orchestrates weapon selection,
/// fire rate timing, and shot data construction into a single <see cref="Tick"/> call.
/// </summary>
/// <remarks>
/// <para>
/// Ported from <c>AggressiveBehavior.ShootAndCycleAsync()</c>. The original class
/// mixes DI, Orleans grains, voxel queries, and safe zone checks into a single method.
/// This version is a pure accumulator: it tracks elapsed time and returns whether
/// the NPC should fire, plus the complete <see cref="ShotData"/> if so.
/// </para>
/// <para>
/// Each call to <see cref="Tick"/> performs:
/// <list type="number">
///   <item>Validate inputs (alive, has target, has weapons).</item>
///   <item>Select best weapon for target distance via <see cref="WeaponSelector"/>.</item>
///   <item>Filter compatible ammo by tier and variant.</item>
///   <item>Compute fire interval via <see cref="WeaponFireRateCalculator"/>.</item>
///   <item>Accumulate delta time; if interval reached, produce <see cref="ShotData"/>.</item>
/// </list>
/// </para>
/// <para>
/// <b>Not handled by this class</b> (consumer responsibility):
/// <list type="bullet">
///   <item>Safe zone checks — call <see cref="Interfaces.ISafeZoneService"/> before/after Tick.</item>
///   <item>Hit position queries — call <see cref="Interfaces.IHitPositionService"/> and set on ShotData.</item>
///   <item>Shot dispatch — pass <see cref="ShotData"/> to <see cref="Interfaces.IShotDispatchService"/>.</item>
/// </list>
/// </para>
/// </remarks>
public class FiringSimulator
{
    private double _accumulatedTime;
    private readonly Random _random;

    /// <summary>
    /// Creates a new firing simulator instance.
    /// </summary>
    /// <param name="random">Optional random number generator for ammo selection. If null, a new <see cref="Random"/> is created.</param>
    public FiringSimulator(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <summary>Current accumulated time since last shot, in seconds.</summary>
    public double AccumulatedTime => _accumulatedTime;

    /// <summary>
    /// Processes a single firing tick. Accumulates delta time and fires when the interval is reached.
    /// </summary>
    /// <param name="input">All inputs for this tick including NPC state, target, weapons, and timing.</param>
    /// <returns>
    /// A <see cref="FiringOutput"/> indicating whether the NPC should fire, along with the
    /// <see cref="ShotData"/> if firing, or a <see cref="FiringSuppressedReason"/> if not.
    /// </returns>
    /// <remarks>
    /// The caller is responsible for safe zone checks (via <see cref="Interfaces.ISafeZoneService"/>),
    /// hit position resolution (via <see cref="Interfaces.IHitPositionService"/>), and shot dispatch
    /// (via <see cref="Interfaces.IShotDispatchService"/>). This method only computes whether and what to fire.
    /// </remarks>
    public FiringOutput Tick(FiringInput input)
    {
        // --- Guard: not alive ---
        if (!input.IsAlive)
            return Suppressed(FiringSuppressedReason.NotAlive);

        // --- Guard: no target ---
        if (input.TargetConstructId is null || (ulong)input.TargetConstructId.Value == 0)
            return Suppressed(FiringSuppressedReason.NoTarget);

        // --- Guard: no weapons ---
        if (input.Weapons.Count == 0)
            return Suppressed(FiringSuppressedReason.NoWeapons);

        // --- Guard: all weapons destroyed ---
        if (!WeaponSelector.HasAnyFunctionalWeapons(input.WeaponEffectiveness))
            return Suppressed(FiringSuppressedReason.AllWeaponsDestroyed);

        // --- Range check ---
        var distance = input.Position.Dist(input.TargetPosition);
        if (distance > input.MaxEngagementRange)
            return Suppressed(FiringSuppressedReason.OutOfRange);

        // --- Select weapon ---
        var weapon = WeaponSelector.SelectBestWeapon(input.Weapons, input.WeaponEffectiveness, distance);
        if (weapon == null)
            return Suppressed(FiringSuppressedReason.AllWeaponsDestroyed);

        // --- Filter ammo ---
        var compatibleAmmo = weapon.AmmoItems
            .Where(a => a.Level == input.AmmoTier &&
                        a.ItemTypeName.Contains(input.AmmoVariant, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (compatibleAmmo.Count == 0)
            return Suppressed(FiringSuppressedReason.NoCompatibleAmmo, weapon);

        var ammo = compatibleAmmo[_random.Next(compatibleAmmo.Count)];

        // --- Effectiveness ---
        var (functionalCount, totalCount) = WeaponSelector.GetEffectivenessFactors(
            input.WeaponEffectiveness, weapon.ItemTypeName);
        var functionalFactor = Math.Clamp((double)functionalCount / totalCount, 0d, 1d);
        var clampedCount = Math.Clamp(functionalCount, 0, input.MaxWeaponCount);

        // --- Fire rate ---
        var fireInterval = WeaponFireRateCalculator.CalculateFireInterval(
            weapon, ammo, input.Modifiers, clampedCount, input.MaxWeaponCount);

        // --- Accumulate ---
        _accumulatedTime += input.DeltaTime;

        if (_accumulatedTime < fireInterval)
        {
            return new FiringOutput
            {
                ShouldFire = false,
                SelectedWeapon = weapon,
                FireInterval = fireInterval,
                AccumulatedTime = _accumulatedTime,
                FunctionalWeaponFactor = functionalFactor,
                SuppressedReason = FiringSuppressedReason.CooldownNotReached,
            };
        }

        // --- FIRE ---
        _accumulatedTime = 0;

        var mod = input.Modifiers;
        var shot = new ShotData
        {
            WeaponDisplayName = weapon.DisplayName,
            ShooterConstructId = input.ConstructId,
            ShooterPosition = input.Position,
            ShooterConstructSize = input.ConstructSize,
            TargetConstructId = input.TargetConstructId.Value,
            TargetPosition = input.TargetPosition,
            HitPosition = default, // Consumer sets via IHitPositionService

            Damage = weapon.BaseDamage * mod.Damage,
            Range = weapon.BaseOptimalDistance * mod.OptimalDistance + weapon.FalloffDistance * mod.FalloffDistance,
            BaseAccuracy = weapon.BaseAccuracy * mod.Accuracy,
            BaseOptimalDistance = weapon.BaseOptimalDistance * mod.OptimalDistance,
            BaseOptimalTracking = weapon.BaseOptimalTracking * mod.OptimalTracking,
            BaseOptimalAimingCone = weapon.BaseOptimalAimingCone * mod.OptimalAimingCone,
            FalloffDistance = weapon.FalloffDistance * mod.FalloffDistance,
            FalloffTracking = weapon.FalloffTracking * mod.FalloffTracking,
            FalloffAimingCone = weapon.FalloffAimingCone * mod.FalloffAimingCone,
            OptimalCrossSectionDiameter = weapon.OptimalCrossSectionDiameter,
            FireCooldown = fireInterval,
            CrossSection = 5,

            AmmoItemTypeName = ammo.ItemTypeName,
            WeaponItemTypeName = weapon.ItemTypeName,
            WeaponCount = clampedCount,
        };

        return new FiringOutput
        {
            ShouldFire = true,
            Shot = shot,
            SelectedWeapon = weapon,
            FireInterval = fireInterval,
            AccumulatedTime = 0,
            FunctionalWeaponFactor = functionalFactor,
        };
    }

    /// <summary>Resets the shot accumulator (e.g., when target changes or safe zone entered).</summary>
    public void ResetAccumulator() => _accumulatedTime = 0;

    private static FiringOutput Suppressed(FiringSuppressedReason reason, WeaponStats? weapon = null)
    {
        return new FiringOutput
        {
            ShouldFire = false,
            SuppressedReason = reason,
            SelectedWeapon = weapon,
        };
    }
}
