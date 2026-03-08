using NpcCommonLib.Data;
using NpcCommonLib.Math;

namespace NpcTargetingLib.Data;

/// <summary>
/// All inputs needed for a single targeting tick.
/// </summary>
public class TargetingInput
{
    /// <summary>NPC's own construct identifier (excluded from radar results).</summary>
    public required ConstructId ConstructId { get; set; }

    /// <summary>NPC's current world-space position in metres.</summary>
    public required Vec3 Position { get; set; }

    /// <summary>NPC's home/spawn position — used as fallback move target when no contacts.</summary>
    public required Vec3 StartPosition { get; set; }

    /// <summary>Radar contacts from the most recent scan.</summary>
    public required IReadOnlyList<ScanContact> Contacts { get; set; }

    /// <summary>Seconds since last tick.</summary>
    public required double DeltaTime { get; set; }

    /// <summary>
    /// Target's linear velocity in m/s (for lead prediction).
    /// Zero vector if no target or velocity unknown.
    /// </summary>
    public Vec3 TargetLinearVelocity { get; set; }

    /// <summary>
    /// Target's acceleration in m/s² (for lead prediction).
    /// Zero vector if unknown.
    /// </summary>
    public Vec3 TargetAcceleration { get; set; }

    /// <summary>
    /// Weapon optimal range in metres. Used to compute prediction seconds
    /// and to determine whether target is inside/outside optimal range.
    /// </summary>
    public double WeaponOptimalRange { get; set; }

    /// <summary>
    /// Maximum visibility distance in metres. Targets beyond this are ignored.
    /// Default: 10 SU (2,000,000 m).
    /// </summary>
    public double MaxVisibilityDistance { get; set; } = 2_000_000;

    /// <summary>
    /// How long a random target selection is held before re-rolling, in seconds.
    /// Only used by RandomTargetStrategy. Default: 30s.
    /// </summary>
    public double DecisionHoldSeconds { get; set; } = 30;
}
