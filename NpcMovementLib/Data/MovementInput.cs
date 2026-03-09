using System.Numerics;
using NpcCommonLib.Math;

namespace NpcMovementLib.Data;

/// <summary>
/// Contains all per-tick inputs needed by <see cref="MovementSimulator"/> to compute the next NPC position,
/// velocity, and rotation. Combines current NPC state, target information, movement parameters,
/// engine state, and velocity goal modifiers into a single object.
/// </summary>
/// <remarks>
/// This is the NpcMovementLib equivalent of the fields spread across
/// <c>BehaviorContext</c> and <c>PrefabItem</c> in the original game code.
/// Populate this struct each tick before calling <see cref="MovementSimulator.Tick"/>.
/// </remarks>
public class MovementInput
{
    // ── Current NPC state ──────────────────────────────────────────

    /// <summary>
    /// Current world-space position of the NPC construct, in metres.
    /// </summary>
    /// <remarks>
    /// Corresponds to <c>BehaviorContext.Position</c> in the original code.
    /// Updated each tick from the construct transform service or from the previous <see cref="MovementOutput.Position"/>.
    /// </remarks>
    public Vec3 Position { get; set; }

    /// <summary>
    /// Current linear velocity of the NPC construct, in m/s.
    /// </summary>
    /// <remarks>
    /// Corresponds to <c>BehaviorContext.Velocity</c>. Passed into the movement strategy as the starting
    /// velocity and updated by acceleration each tick. The magnitude is clamped to <see cref="MaxVelocity"/>.
    /// </remarks>
    public Vec3 Velocity { get; set; }

    /// <summary>
    /// Current orientation of the NPC construct as a unit quaternion.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="MovementSimulator"/> to derive the forward direction vector, which determines
    /// the thrust direction when <see cref="RealismFactor"/> is greater than zero.
    /// Defaults to <see cref="Quaternion.Identity"/> (forward along +Z).
    /// </remarks>
    public Quaternion Rotation { get; set; } = Quaternion.Identity;

    // ── Target ─────────────────────────────────────────────────────

    /// <summary>
    /// World-space position the NPC is currently steering toward, in metres.
    /// </summary>
    /// <remarks>
    /// In combat this is typically the predicted intercept position of the target construct
    /// (see <c>CalculateTargetMovePositionWithOffsetEffect</c> in the original code).
    /// When an override target move position is active, this reflects the override point instead.
    /// The direction from <see cref="Position"/> to this point defines the move direction used
    /// for acceleration and rotation alignment.
    /// </remarks>
    public Vec3 TargetMovePosition { get; set; }

    // ── Timing ─────────────────────────────────────────────────────

    /// <summary>
    /// Elapsed time since the last movement tick, in seconds.
    /// </summary>
    /// <remarks>
    /// In the original game code, <c>BehaviorContext.DeltaTime</c> is clamped to the range [1/60, 1].
    /// This value scales all velocity changes, displacement calculations, and rotation interpolation.
    /// </remarks>
    public double DeltaTime { get; set; }

    // ── Movement parameters ────────────────────────────────────────

    /// <summary>
    /// Base acceleration magnitude in G-force units (1 G = 9.81 m/s²).
    /// Controls how quickly the NPC can change velocity per tick.
    /// </summary>
    /// <remarks>
    /// Converted to m/s² in <see cref="MovementSimulator"/> via <c>AccelerationG * 9.81</c> for physics
    /// calculations (thrust, braking forces). A separate "MPS" conversion via <see cref="GetAccelerationMps"/>
    /// (using factor 3.6) is used solely for braking-distance estimation.
    /// Typical values range from 1 G (slow freighter) to 30+ G (agile fighter).
    /// Defaults to 15 G.
    /// </remarks>
    public double AccelerationG { get; set; } = 15;

    /// <summary>
    /// Maximum allowed speed of the NPC, in km/h.
    /// </summary>
    /// <remarks>
    /// Converted to m/s via <see cref="MaxVelocity"/> (<c>MaxSpeedKph / 3.6</c>).
    /// Acts as the hard upper bound on velocity magnitude. The NPC's velocity is clamped to this value
    /// after each movement strategy tick.
    /// Corresponds to <c>PrefabItem.MaxSpeedKph</c> in the original code. Default is 20 000 km/h.
    /// </remarks>
    public double MaxSpeedKph { get; set; } = 20000;

    /// <summary>
    /// Minimum speed the NPC should maintain during combat manoeuvring, in km/h.
    /// </summary>
    /// <remarks>
    /// Converted to m/s via <see cref="MinVelocity"/> (<c>MinSpeedKph / 3.6</c>).
    /// Used by <see cref="VelocityGoalCalculator"/> as the floor when computing velocity goals,
    /// ensuring the NPC never slows down below a minimum combat speed.
    /// Corresponds to <c>PrefabItem.MinSpeedKph</c>. Default is 2 000 km/h.
    /// </remarks>
    public double MinSpeedKph { get; set; } = 2000;

    /// <summary>
    /// Rotation interpolation speed factor, in the range [0, 1].
    /// </summary>
    /// <remarks>
    /// Multiplied by <see cref="DeltaTime"/> to produce the <c>Quaternion.Slerp</c> t-parameter
    /// each tick. A value of 0 means no rotation; 1 means the ship instantly faces its target direction
    /// (within one tick). Typical values are 0.1 to 1.0.
    /// Corresponds to <c>PrefabItem.RotationSpeed</c>. Default is 0.5.
    /// </remarks>
    public float RotationSpeed { get; set; } = 0.5f;

    /// <summary>
    /// Blend factor between forward-thrust (realistic) and direct-to-target (arcade) acceleration.
    /// Range [0, 1] where 0 = pure direct steering and 1 = pure forward thrust.
    /// </summary>
    /// <remarks>
    /// In <see cref="MovementSimulator"/>, acceleration is split into two components:
    /// <list type="bullet">
    ///   <item><c>accelForward = forward * acceleration * RealismFactor</c> (along ship's nose)</item>
    ///   <item><c>accelMove = moveDirection * acceleration * (1 - RealismFactor)</c> (directly toward target)</item>
    /// </list>
    /// At 0 the NPC steers instantly toward the target (arcade feel); at 1 it can only accelerate
    /// along its current heading, requiring rotation first (realistic flight).
    /// Corresponds to <c>PrefabItem.RealismFactor</c> and <c>BehaviorContext.RealismFactor</c>.
    /// </remarks>
    public double RealismFactor { get; set; }

    // ── Engine ──────────────────────────────────────────────────────

    /// <summary>
    /// Engine power multiplier applied to the acceleration vector in movement strategies.
    /// Range [0, 1] where 0 = engines off and 1 = full power.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="Strategies.BurnToTargetStrategy"/> as <c>acceleration * EnginePower</c>.
    /// In the original code this is stored as a dynamic property on <c>BehaviorContext</c>
    /// (<c>EnginePowerProperty</c>). When set to 0 or when <see cref="IsBraking"/> is true,
    /// <see cref="MovementSimulator"/> switches to the <see cref="Strategies.BrakingStrategy"/>.
    /// Default is 1 (full power).
    /// </remarks>
    public double EnginePower { get; set; } = 1;

    /// <summary>
    /// When <c>true</c>, the NPC applies braking instead of thrusting.
    /// </summary>
    /// <remarks>
    /// Causes <see cref="MovementSimulator"/> to select the <see cref="Strategies.BrakingStrategy"/>
    /// instead of the default movement strategy. In the original code, braking is activated
    /// via <c>ApplyBrakesMovementEffect</c> when the NPC needs to decelerate
    /// (e.g., approaching a waypoint or when engine power drops to zero).
    /// </remarks>
    public bool IsBraking { get; set; }

    // ── Velocity goal inputs ───────────────────────────────────────

    /// <summary>
    /// Configuration modifiers that control how <see cref="VelocityGoalCalculator"/> adjusts
    /// the NPC's target speed based on distance, weapon range, and relative velocity direction.
    /// </summary>
    /// <remarks>
    /// Mirrors <c>BehaviorModifiers.Velocity</c> in the original code. When
    /// <see cref="VelocityModifiers.Enabled"/> is <c>false</c>, the velocity goal always equals
    /// <see cref="MaxVelocity"/>.
    /// </remarks>
    public VelocityModifiers Modifiers { get; set; } = new();

    /// <summary>
    /// Distance from the NPC to the enemy target construct, in metres.
    /// </summary>
    /// <remarks>
    /// Not the same as <see cref="TargetMoveDistance"/>. This is the straight-line distance
    /// to the actual target construct (not to the move position). Used by
    /// <see cref="VelocityGoalCalculator"/> to determine which range bracket applies
    /// (inside optimal, outside optimal, outside 2x optimal).
    /// Corresponds to <c>BehaviorContext.TargetDistance</c>.
    /// </remarks>
    public double TargetDistance { get; set; }

    /// <summary>
    /// Current linear velocity of the target construct, in m/s.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="VelocityGoalCalculator"/> to compute a base velocity goal
    /// when inside weapon range -- the NPC tries to match the target's speed.
    /// Also used to compute the NPC-vs-target velocity dot product that selects
    /// the positive or negative modifier branch.
    /// Corresponds to <c>BehaviorContext.TargetLinearVelocity</c>.
    /// </remarks>
    public Vec3 TargetLinearVelocity { get; set; }

    /// <summary>
    /// Optimal firing range of the NPC's best weapon, in metres.
    /// </summary>
    /// <remarks>
    /// Defines the range brackets used by <see cref="VelocityGoalCalculator"/>:
    /// <list type="bullet">
    ///   <item>Inside optimal range: NPC matches target speed</item>
    ///   <item>Outside optimal range: NPC uses <see cref="VelocityModifiers.OutsideOptimalRange"/> modifiers</item>
    ///   <item>Outside 2x optimal range: NPC uses <see cref="VelocityModifiers.OutsideOptimalRange2X"/> modifiers</item>
    /// </list>
    /// In the original code this is computed by <c>BehaviorContext.GetBestWeaponOptimalRange()</c>
    /// using the weapon's half-falloff firing distance.
    /// </remarks>
    public double WeaponOptimalRange { get; set; }

    /// <summary>
    /// When <c>true</c>, the NPC is navigating to a manually-specified override position
    /// (e.g., a waypoint) rather than following a combat target.
    /// </summary>
    /// <remarks>
    /// When set, <see cref="VelocityGoalCalculator"/> uses a simplified approach:
    /// if within braking distance of the override position, the velocity goal is 0 (full stop);
    /// otherwise, it is <see cref="MaxVelocity"/>.
    /// Corresponds to <c>BehaviorContext.OverrideTargetMovePosition.HasValue</c>.
    /// </remarks>
    public bool HasOverrideTargetMovePosition { get; set; }

    /// <summary>
    /// Distance from the NPC to the override target move position, in metres.
    /// </summary>
    /// <remarks>
    /// Only meaningful when <see cref="HasOverrideTargetMovePosition"/> is <c>true</c>.
    /// Used by <see cref="VelocityGoalCalculator"/> to decide whether to brake to a stop
    /// at the override position. Corresponds to the result of <c>BehaviorContext.GetMovePositionDistance()</c>.
    /// </remarks>
    public double OverrideMovePositionDistance { get; set; }

    // ── Booster ────────────────────────────────────────────────────

    /// <summary>
    /// Whether the NPC's booster is currently firing.
    /// </summary>
    /// <remarks>
    /// When <c>true</c> and <see cref="VelocityModifiers.BoosterEnabled"/> is also <c>true</c>,
    /// <see cref="GetAccelerationG"/> adds <see cref="VelocityModifiers.BoosterAccelerationG"/>
    /// to the base <see cref="AccelerationG"/>, giving the NPC a temporary speed burst.
    /// </remarks>
    public bool BoosterActive { get; set; }

    // ── BurnToTarget delta-V clamping ──────────────────────────────

    /// <summary>
    /// Velocity from the previous tick, used for delta-V clamping in <see cref="Strategies.BurnToTargetStrategy"/>.
    /// </summary>
    /// <remarks>
    /// When non-null, <see cref="Strategies.BurnToTargetStrategy"/> computes <c>deltaV = velocity - PreviousVelocity</c>
    /// and clamps it to <c>acceleration * deltaTime</c> to prevent unrealistic instantaneous velocity jumps.
    /// Corresponds to the <c>V0</c> property stored on <c>BehaviorContext</c> in the original
    /// <c>BurnToTargetMovementEffect</c>.
    /// When <c>null</c>, the current computed velocity is used as V0 (no clamping on the first tick).
    /// </remarks>
    public Vec3? PreviousVelocity { get; set; }

    // ── Derived helpers ────────────────────────────────────────────

    /// <summary>
    /// Minimum speed in m/s, derived from <see cref="MinSpeedKph"/> via <c>MinSpeedKph / 3.6</c>.
    /// </summary>
    public double MinVelocity => MinSpeedKph / 3.6d;

    /// <summary>
    /// Maximum speed in m/s, derived from <see cref="MaxSpeedKph"/> via <c>MaxSpeedKph / 3.6</c>.
    /// </summary>
    public double MaxVelocity => MaxSpeedKph / 3.6d;

    /// <summary>
    /// Effective acceleration in the project's internal "MPS" unit, derived from <see cref="GetAccelerationMps"/>.
    /// Used for braking-distance calculations.
    /// </summary>
    /// <remarks>
    /// This is <b>not</b> a standard m/s² conversion. See <see cref="GetAccelerationMps"/> for details.
    /// </remarks>
    public double AccelerationMps => GetAccelerationMps();

    /// <summary>
    /// Straight-line distance from the NPC's <see cref="Position"/> to <see cref="TargetMovePosition"/>, in metres.
    /// </summary>
    /// <remarks>
    /// Recomputed on access. Used by <see cref="MovementSimulator"/> as the <c>Distance</c> input to
    /// <see cref="VelocityGoalCalculator"/>, controlling which speed zone the NPC falls into.
    /// </remarks>
    public double TargetMoveDistance => Position.Dist(TargetMovePosition);

    /// <summary>
    /// Returns the effective acceleration in G-force, including the booster contribution
    /// when both <see cref="VelocityModifiers.BoosterEnabled"/> and <see cref="BoosterActive"/> are true.
    /// </summary>
    /// <returns>
    /// <see cref="AccelerationG"/> plus <see cref="VelocityModifiers.BoosterAccelerationG"/> if the booster is active;
    /// otherwise just <see cref="AccelerationG"/>.
    /// </returns>
    public double GetAccelerationG()
    {
        var boosterG = 0d;
        if (Modifiers.BoosterEnabled && BoosterActive)
        {
            boosterG = Modifiers.BoosterAccelerationG;
        }

        return AccelerationG + boosterG;
    }

    /// <summary>
    /// Returns the effective acceleration scaled by a factor of 3.6, used for braking-distance calculations.
    /// </summary>
    /// <remarks>
    /// Despite the name "Mps", this does <b>not</b> return a standard m/s² value.
    /// The factor 3.6 (km/h-to-m/s) is a project-specific scaling convention inherited from
    /// <c>BehaviorContext.GetAccelerationMps()</c>. The result is consumed by
    /// <see cref="Math.VelocityHelper.CalculateBrakingDistance"/> to estimate stopping distance.
    /// For actual physics acceleration (thrust), see <see cref="MovementSimulator"/> which uses
    /// <c>AccelerationG * 9.81</c>.
    /// </remarks>
    /// <returns><see cref="GetAccelerationG"/> multiplied by 3.6.</returns>
    public double GetAccelerationMps() => GetAccelerationG() * 3.6d;
}
