using NpcMovementLib.Data;
using NpcMovementLib.Math;

namespace NpcWeaponLib.Data;

/// <summary>
/// All inputs needed for a single weapon firing tick.
/// Analogous to <see cref="NpcMovementLib.Data.MovementInput"/> for the movement system.
/// </summary>
public class FiringInput
{
    // --- NPC State ---

    /// <summary>NPC construct identifier.</summary>
    public required ConstructId ConstructId { get; set; }

    /// <summary>NPC's current world-space position in metres.</summary>
    public required Vec3 Position { get; set; }

    /// <summary>NPC construct's bounding size (used for hit position fallback).</summary>
    public required ulong ConstructSize { get; set; }

    /// <summary>Whether the NPC is alive. If false, firing is suppressed.</summary>
    public required bool IsAlive { get; set; }

    // --- Target State ---

    /// <summary>Target construct identifier. Null or 0 = no target, skip firing.</summary>
    public ConstructId? TargetConstructId { get; set; }

    /// <summary>Target's world-space position in metres.</summary>
    public Vec3 TargetPosition { get; set; }

    // --- Weapons ---

    /// <summary>All weapons on this NPC construct.</summary>
    public required IReadOnlyList<WeaponStats> Weapons { get; set; }

    /// <summary>Per-weapon-type health data, keyed by ItemTypeName.</summary>
    public required IDictionary<string, IList<WeaponEffectiveness>> WeaponEffectiveness { get; set; }

    /// <summary>Weapon stat multipliers from the NPC prefab.</summary>
    public required WeaponModifiers Modifiers { get; set; }

    // --- Ammo Config ---

    /// <summary>Required ammo tier level (1-5). Filters compatible ammo from weapon's ammo list.</summary>
    public required int AmmoTier { get; set; }

    /// <summary>
    /// Ammo variant name filter (case-insensitive contains match).
    /// E.g., "Kinetic", "Thermic". Filters compatible ammo from weapon's ammo list.
    /// </summary>
    public required string AmmoVariant { get; set; }

    // --- Timing ---

    /// <summary>Seconds since last tick. Accumulated until fire interval is reached.</summary>
    public required double DeltaTime { get; set; }

    /// <summary>Max weapon count from prefab config. Caps the functional weapon count.</summary>
    public required int MaxWeaponCount { get; set; }

    // --- Max Engagement Range ---

    /// <summary>
    /// Maximum engagement distance in metres. Shots beyond this range are suppressed.
    /// Default: 2 SU (400,000 m) matching the original <c>AggressiveBehavior</c>.
    /// </summary>
    public double MaxEngagementRange { get; set; } = 400_000;
}
