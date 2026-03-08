namespace NpcMovementLib.Data;

/// <summary>
/// Static configuration for an NPC's movement capabilities, typically loaded from a prefab definition.
/// Defines the NPC's acceleration, rotation speed, speed limits, and preferred engagement distance.
/// </summary>
/// <remarks>
/// These values correspond to the movement-related fields on <c>PrefabItem</c> in the original game code.
/// They are generally constant for a given NPC type and do not change per-tick.
/// For per-tick mutable state, see <see cref="MovementInput"/>.
/// </remarks>
public class MovementConfig
{
    /// <summary>
    /// Base acceleration magnitude in G-force units (1 G = 9.81 m/s²).
    /// Controls how quickly the NPC can change velocity.
    /// </summary>
    /// <remarks>
    /// Corresponds to <c>PrefabItem.AccelerationG</c>. Higher values produce snappier course
    /// corrections; lower values give smoother, more realistic flight paths.
    /// Typical values range from 1 G (slow freighter) to 30+ G (agile fighter).
    /// Default is 15 G.
    /// </remarks>
    public double AccelerationG { get; set; } = 15;

    /// <summary>
    /// Rotation interpolation speed factor used with <c>Quaternion.Slerp</c>.
    /// </summary>
    /// <remarks>
    /// Multiplied by delta time each tick to determine how quickly the NPC rotates toward
    /// its target heading. A value of 0 means no rotation; approaching 1 means nearly instant
    /// reorientation per tick.
    /// Corresponds to <c>PrefabItem.RotationSpeed</c>. Default is 0.5.
    /// </remarks>
    public float RotationSpeed { get; set; } = 0.5f;

    /// <summary>
    /// Minimum speed the NPC should maintain during combat manoeuvring, in km/h.
    /// </summary>
    /// <remarks>
    /// Acts as a floor for the velocity goal computed by <see cref="VelocityGoalCalculator"/>.
    /// Prevents the NPC from slowing to a crawl during close-range engagements.
    /// Corresponds to <c>PrefabItem.MinSpeedKph</c>. Default is 2 000 km/h.
    /// </remarks>
    public double MinSpeedKph { get; set; } = 2000;

    /// <summary>
    /// Maximum allowed speed of the NPC, in km/h.
    /// </summary>
    /// <remarks>
    /// Acts as the hard upper bound on velocity magnitude.
    /// Corresponds to <c>PrefabItem.MaxSpeedKph</c>. Default is 20 000 km/h.
    /// </remarks>
    public double MaxSpeedKph { get; set; } = 20000;

    /// <summary>
    /// Blend factor between forward-thrust (realistic) and direct-to-target (arcade) acceleration.
    /// Range [0, 1].
    /// </summary>
    /// <remarks>
    /// At 0 the NPC steers instantly toward the target; at 1 it must rotate first and can only
    /// accelerate along its heading. See <see cref="MovementInput.RealismFactor"/> for full details.
    /// Corresponds to <c>PrefabItem.RealismFactor</c>. Default is 0 (arcade).
    /// </remarks>
    public double RealismFactor { get; set; }

    /// <summary>
    /// Preferred engagement distance the NPC tries to maintain from its target, in metres.
    /// </summary>
    /// <remarks>
    /// Used as the <c>TargetDistance</c> input to <see cref="VelocityGoalCalculator"/>.
    /// Corresponds to <c>PrefabItem.TargetDistance</c>. Default is 20 000 m.
    /// </remarks>
    public double TargetDistance { get; set; } = 20000;

    /// <summary>
    /// Minimum speed in m/s, derived from <see cref="MinSpeedKph"/> via <c>MinSpeedKph / 3.6</c>.
    /// </summary>
    public double MinVelocity => MinSpeedKph / 3.6d;

    /// <summary>
    /// Maximum speed in m/s, derived from <see cref="MaxSpeedKph"/> via <c>MaxSpeedKph / 3.6</c>.
    /// </summary>
    public double MaxVelocity => MaxSpeedKph / 3.6d;
}
