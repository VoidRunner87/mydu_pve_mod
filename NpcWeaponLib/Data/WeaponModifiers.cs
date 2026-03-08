namespace NpcWeaponLib.Data;

/// <summary>
/// Multipliers applied to base weapon stats. All default to 1.0 (no modification).
/// </summary>
/// <remarks>
/// Ported from <c>BehaviorModifiers.WeaponModifiers</c>. These are set per-NPC prefab
/// to create weapon variants (e.g., 2x damage boss, 0.5x cycle time rapid-fire ship).
/// Each modifier is multiplied against the corresponding <see cref="WeaponStats"/> base value
/// when constructing the final shot parameters.
/// </remarks>
public class WeaponModifiers
{
    /// <summary>Multiplier for <see cref="WeaponStats.BaseDamage"/>. Default 1.0 (no change).</summary>
    public float Damage { get; set; } = 1;

    /// <summary>Multiplier for <see cref="WeaponStats.BaseAccuracy"/>. Default 1.0 (no change).</summary>
    public float Accuracy { get; set; } = 1;

    /// <summary>Multiplier for <see cref="WeaponStats.BaseCycleTime"/>. Lower values = faster firing. Default 1.0.</summary>
    public float CycleTime { get; set; } = 1;

    /// <summary>Multiplier for <see cref="WeaponStats.BaseOptimalDistance"/>. Default 1.0 (no change).</summary>
    public float OptimalDistance { get; set; } = 1;

    /// <summary>Multiplier for <see cref="WeaponStats.FalloffDistance"/>. Default 1.0 (no change).</summary>
    public float FalloffDistance { get; set; } = 1;

    /// <summary>Multiplier for <see cref="WeaponStats.FalloffAimingCone"/>. Default 1.0 (no change).</summary>
    public float FalloffAimingCone { get; set; } = 1;

    /// <summary>Multiplier for <see cref="WeaponStats.FalloffTracking"/>. Default 1.0 (no change).</summary>
    public float FalloffTracking { get; set; } = 1;

    /// <summary>Multiplier for <see cref="WeaponStats.BaseOptimalTracking"/>. Default 1.0 (no change).</summary>
    public float OptimalTracking { get; set; } = 1;

    /// <summary>Multiplier for <see cref="WeaponStats.BaseOptimalAimingCone"/>. Default 1.0 (no change).</summary>
    public float OptimalAimingCone { get; set; } = 1;
}
