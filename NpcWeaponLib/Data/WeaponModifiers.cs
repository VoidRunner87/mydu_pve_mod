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
    public float Damage { get; set; } = 1;
    public float Accuracy { get; set; } = 1;
    public float CycleTime { get; set; } = 1;
    public float OptimalDistance { get; set; } = 1;
    public float FalloffDistance { get; set; } = 1;
    public float FalloffAimingCone { get; set; } = 1;
    public float FalloffTracking { get; set; } = 1;
    public float OptimalTracking { get; set; } = 1;
    public float OptimalAimingCone { get; set; } = 1;
}
