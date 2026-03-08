namespace NpcMovementLib.Data;

/// <summary>
/// Configuration parameters that control how <see cref="VelocityGoalCalculator"/> adjusts the NPC's
/// target speed based on distance to target, weapon optimal range, and relative velocity direction.
/// </summary>
/// <remarks>
/// This is the NpcMovementLib equivalent of <c>BehaviorModifiers.VelocityModifiers</c> in the original
/// game code. The velocity goal system divides space around the target into range brackets
/// (inside optimal, outside optimal, outside 2x optimal, far, and too-close) and applies
/// different speed scaling rules in each zone.
/// </remarks>
public class VelocityModifiers
{
    /// <summary>
    /// Length of one Spatial Unit (SU) in metres. Used to convert <see cref="FarDistanceSu"/> to metres.
    /// </summary>
    /// <remarks>
    /// In Dual Universe, 1 SU = 200 000 m. This constant matches
    /// <c>DistanceHelpers.OneSuInMeters</c> in the original code.
    /// </remarks>
    public const long OneSuInMeters = 200000;

    /// <summary>
    /// Master switch for the velocity goal system. When <c>false</c>,
    /// <see cref="VelocityGoalCalculator"/> always returns <see cref="MovementInput.MaxVelocity"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>. Set to <c>false</c> to make the NPC always fly at maximum speed,
    /// ignoring all range-based speed adjustments.
    /// </remarks>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether the NPC has a booster module that can provide additional acceleration.
    /// </summary>
    /// <remarks>
    /// When <c>true</c> and <see cref="MovementInput.BoosterActive"/> is also <c>true</c>,
    /// <see cref="MovementInput.GetAccelerationG"/> adds <see cref="BoosterAccelerationG"/>
    /// to the base acceleration. Defaults to <c>false</c> (no booster).
    /// </remarks>
    public bool BoosterEnabled { get; set; } = false;

    /// <summary>
    /// Additional acceleration provided by the booster, in G-force units (1 G = 9.81 m/s²).
    /// </summary>
    /// <remarks>
    /// Only applied when both <see cref="BoosterEnabled"/> and <see cref="MovementInput.BoosterActive"/>
    /// are <c>true</c>. Added to <see cref="MovementInput.AccelerationG"/> in
    /// <see cref="MovementInput.GetAccelerationG"/>.
    /// Default is 5 G.
    /// </remarks>
    public double BoosterAccelerationG { get; set; } = 5d;

    /// <summary>
    /// Distance threshold beyond which the NPC flies at maximum speed, in Spatial Units (SU).
    /// </summary>
    /// <remarks>
    /// When the NPC-to-target-move-position distance exceeds this value (converted to metres via
    /// <see cref="GetFarDistanceM"/>), the velocity goal is set to <see cref="MovementInput.MaxVelocity"/>
    /// regardless of weapon range considerations. This prevents unnecessary speed throttling during
    /// long-range approach phases.
    /// Default is 1.5 SU (300 000 m).
    /// </remarks>
    public double FarDistanceSu { get; set; } = 1.5d;

    /// <summary>
    /// Distance threshold below which the NPC flies at maximum speed to disengage, in metres.
    /// </summary>
    /// <remarks>
    /// When the NPC is inside weapon optimal range but closer than this distance to the target
    /// move position, the velocity goal jumps to <see cref="MovementInput.MaxVelocity"/>.
    /// This prevents the NPC from orbiting too tightly or colliding with the target.
    /// In the original code this corresponds to <c>VelocityModifiers.TooCloseDistanceM</c>.
    /// Default is 15 000 m.
    /// </remarks>
    public double TooCloseDistanceM { get; set; } = 15000;

    /// <summary>
    /// Multiplier applied to the computed braking distance when deciding whether the NPC
    /// should start decelerating. Values greater than 1 create a safety margin.
    /// </summary>
    /// <remarks>
    /// Used in two places:
    /// <list type="bullet">
    ///   <item>In the normal velocity goal calculation, if <c>distance &gt; brakingDistance * BrakeDistanceFactor</c>,
    ///         the "outside 2x optimal range" speed rules apply.</item>
    ///   <item>In override-move-position mode, if <c>distance &lt;= brakingDistance * BrakeDistanceFactor</c>,
    ///         the velocity goal is set to 0 (full stop).</item>
    /// </list>
    /// Default is 2.0 (begin braking at twice the minimum physical braking distance).
    /// </remarks>
    public double BrakeDistanceFactor { get; set; } = 2d;

    /// <summary>
    /// Velocity scaling modifiers applied when the NPC is beyond 2x the weapon's optimal range.
    /// </summary>
    /// <remarks>
    /// The <see cref="ModifierByDotProduct.Positive"/> multiplier is used when the NPC's velocity
    /// direction aligns with the target's velocity (same heading); <see cref="ModifierByDotProduct.Negative"/>
    /// is used when they are heading in opposite directions.
    /// The base velocity in this zone is the target's speed (clamped to [MinVelocity, MaxVelocity]),
    /// or <c>MaxVelocity / <see cref="OutsideOptimalRange2XAlpha"/></c> if the target is slow.
    /// Defaults: Negative = 0.5, Positive = 1.5.
    /// </remarks>
    public ModifierByDotProduct OutsideOptimalRange2X { get; set; }
        = new() { Negative = 0.5d, Positive = 1.5d };

    /// <summary>
    /// Velocity scaling modifiers applied when the NPC is between 1x and 2x the weapon's optimal range.
    /// </summary>
    /// <remarks>
    /// Same structure as <see cref="OutsideOptimalRange2X"/>. The base velocity is the target's speed
    /// (clamped), or <c>MaxVelocity / <see cref="OutsideOptimalRangeAlpha"/></c> if the target is slow.
    /// Defaults: Negative = 0.25, Positive = 1.2.
    /// </remarks>
    public ModifierByDotProduct OutsideOptimalRange { get; set; }
        = new() { Negative = 0.25d, Positive = 1.2d };

    /// <summary>
    /// Velocity scaling modifiers applied when the NPC is inside the weapon's optimal range
    /// and beyond <see cref="TooCloseDistanceM"/>.
    /// </summary>
    /// <remarks>
    /// At default values (1.0 / 1.0), no additional scaling is applied -- the NPC simply
    /// tries to match the target's speed. Adjust to make the NPC fly faster or slower
    /// than the target during close combat.
    /// Defaults: Negative = 1.0, Positive = 1.0.
    /// </remarks>
    public ModifierByDotProduct InsideOptimalRange { get; set; }
        = new() { Negative = 1d, Positive = 1d };

    /// <summary>
    /// Divisor applied to <see cref="MovementInput.MaxVelocity"/> to compute a fallback base velocity
    /// when the target is moving slowly (below <see cref="MovementInput.MinVelocity"/>) and the NPC
    /// is in the "outside 2x optimal range" zone.
    /// </summary>
    /// <remarks>
    /// A higher value produces a slower fallback speed. For example, with the default of 2,
    /// the NPC cruises at half max speed when the target is nearly stationary and far away.
    /// </remarks>
    public double OutsideOptimalRange2XAlpha { get; set; } = 2;

    /// <summary>
    /// Divisor applied to <see cref="MovementInput.MaxVelocity"/> to compute a fallback base velocity
    /// when the target is moving slowly (below <see cref="MovementInput.MinVelocity"/>) and the NPC
    /// is in the "outside optimal range" (1x--2x) zone.
    /// </summary>
    /// <remarks>
    /// A higher value produces a slower fallback speed. For example, with the default of 4,
    /// the NPC cruises at one-quarter max speed when the target is nearly stationary and
    /// at medium range. This is more conservative than <see cref="OutsideOptimalRange2XAlpha"/>
    /// because the NPC is closer and needs to avoid overshooting.
    /// </remarks>
    public double OutsideOptimalRangeAlpha { get; set; } = 4;

    /// <summary>
    /// Converts <see cref="FarDistanceSu"/> to metres by multiplying by <see cref="OneSuInMeters"/>.
    /// </summary>
    /// <returns>The far-distance threshold in metres.</returns>
    public double GetFarDistanceM() => FarDistanceSu * OneSuInMeters;
}

/// <summary>
/// A pair of multipliers selected based on the dot product between the NPC's velocity direction
/// and the target's velocity direction.
/// </summary>
/// <remarks>
/// When the dot product is negative (NPC and target heading in opposite directions),
/// <see cref="Negative"/> is used. When positive (same general heading), <see cref="Positive"/> is used.
/// This allows the velocity goal to be adjusted based on whether the NPC is chasing the target
/// or flying head-on toward it.
/// </remarks>
public struct ModifierByDotProduct
{
    /// <summary>
    /// Multiplier applied to the base velocity goal when the NPC and target are heading
    /// in the same general direction (dot product >= 0).
    /// </summary>
    /// <remarks>
    /// Values greater than 1.0 make the NPC fly faster than the base velocity;
    /// values less than 1.0 make it fly slower. For example, 1.5 means 50% faster.
    /// </remarks>
    public required double Positive { get; set; }

    /// <summary>
    /// Multiplier applied to the base velocity goal when the NPC and target are heading
    /// in opposite directions (dot product &lt; 0).
    /// </summary>
    /// <remarks>
    /// Typically set lower than <see cref="Positive"/> to slow the NPC when it is on a
    /// head-on approach, reducing closing speed and giving more time to manoeuvre.
    /// </remarks>
    public required double Negative { get; set; }
}
