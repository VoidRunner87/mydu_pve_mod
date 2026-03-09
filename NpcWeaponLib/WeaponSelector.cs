namespace NpcWeaponLib;

using NpcWeaponLib.Data;

/// <summary>
/// Selects the best weapon for engagement based on target distance and weapon health.
/// All methods are pure functions with no side effects.
/// </summary>
/// <remarks>
/// Ported from <c>ConstructDamageData.GetBestWeaponByTargetDistance()</c> and
/// <c>BehaviorContext.GetBestFunctionalWeaponByTargetDistance()</c>.
///
/// Selection algorithm:
/// <list type="number">
///   <item>Filter to only functional weapons (hitpoints &gt; 1%).</item>
///   <item>For each functional weapon, compute half-falloff distance (optimal + falloff/2).</item>
///   <item>Select the weapon whose half-falloff distance is closest to target distance.</item>
/// </list>
/// </remarks>
public static class WeaponSelector
{
    /// <summary>
    /// Picks the best functional weapon for the given target distance.
    /// Returns null if no functional weapons remain.
    /// </summary>
    /// <param name="weapons">All weapons on the construct.</param>
    /// <param name="effectiveness">Per-weapon health data, keyed by ItemTypeName.</param>
    /// <param name="targetDistance">Distance to target in metres.</param>
    /// <returns>The best-matching functional weapon, or <c>null</c> if no functional weapons remain.</returns>
    public static WeaponStats? SelectBestWeapon(
        IEnumerable<WeaponStats> weapons,
        IDictionary<string, IList<WeaponEffectiveness>> effectiveness,
        double targetDistance)
    {
        var functionalNames = effectiveness
            .SelectMany(kvp => kvp.Value)
            .Where(e => !e.IsDestroyed())
            .Select(e => e.Name)
            .ToHashSet();

        var functionalWeapons = weapons.Where(w => functionalNames.Contains(w.ItemTypeName));

        return functionalWeapons
            .Select(w => new { Weapon = w, Delta = Math.Abs(w.GetHalfFalloffDistance() - targetDistance) })
            .MinBy(x => x.Delta)?.Weapon;
    }

    /// <summary>
    /// Returns (functionalCount, totalCount) for a specific weapon type.
    /// </summary>
    /// <param name="effectiveness">Per-weapon health data, keyed by ItemTypeName.</param>
    /// <param name="itemTypeName">Weapon type to query.</param>
    /// <returns>
    /// A tuple of (FunctionalCount, TotalCount). Returns (0, 1) if the weapon type is not found,
    /// to avoid division by zero.
    /// </returns>
    public static (int FunctionalCount, int TotalCount) GetEffectivenessFactors(
        IDictionary<string, IList<WeaponEffectiveness>> effectiveness,
        string itemTypeName)
    {
        if (!effectiveness.TryGetValue(itemTypeName, out var list) || list.Count == 0)
            return (0, 1);

        return (list.Count(x => !x.IsDestroyed()), list.Count);
    }

    /// <summary>Returns true if any weapon across all types is still functional.</summary>
    /// <param name="effectiveness">Per-weapon health data, keyed by ItemTypeName.</param>
    /// <returns><c>true</c> if at least one weapon has hitpoints above 1%; <c>false</c> if all destroyed.</returns>
    public static bool HasAnyFunctionalWeapons(IDictionary<string, IList<WeaponEffectiveness>> effectiveness)
    {
        return effectiveness.SelectMany(kvp => kvp.Value).Any(e => !e.IsDestroyed());
    }
}
