namespace NpcWeaponLib.Data;

/// <summary>
/// Health status of an individual weapon element on a construct.
/// </summary>
/// <remarks>
/// Weapons with <see cref="HitPointsRatio"/> at or below 1% are considered destroyed
/// and excluded from firing. The NPC selects only functional weapons.
/// </remarks>
public class WeaponEffectiveness
{
    /// <summary>Internal item type name matching <see cref="WeaponStats.ItemTypeName"/>.</summary>
    public required string Name { get; set; }

    /// <summary>Current hitpoints as a ratio of max (0.0 = destroyed, 1.0 = full health).</summary>
    public required double HitPointsRatio { get; set; }

    /// <summary>Returns true if hitpoints are at or below 1% — weapon is non-functional.</summary>
    public bool IsDestroyed() => HitPointsRatio <= 0.01d;
}
