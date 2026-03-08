using NpcMovementLib.Data;
using NpcCommonLib.Math;

namespace NpcMovementLib;

/// <summary>
/// Computes the NPC's desired speed (velocity goal) based on its distance to the target,
/// weapon optimal range, relative velocity direction, and various configurable modifiers.
/// </summary>
/// <remarks>
/// This is the NpcMovementLib equivalent of <c>BehaviorContext.CalculateVelocityGoal</c> in the
/// original game code. The velocity goal is a scalar speed (m/s) that the movement strategy
/// (e.g., <see cref="Strategies.BurnToTargetStrategy"/>) tries to reach by accelerating or
/// decelerating each tick.
///
/// The calculator divides space into range zones around the target:
/// <list type="number">
///   <item><b>Far</b> (beyond <see cref="VelocityModifiers.FarDistanceSu"/>): fly at max speed.</item>
///   <item><b>Outside 2x optimal range</b> or beyond braking distance: match target speed with
///         <see cref="VelocityModifiers.OutsideOptimalRange2X"/> modifiers.</item>
///   <item><b>Outside optimal range</b> (1x--2x): match target speed with
///         <see cref="VelocityModifiers.OutsideOptimalRange"/> modifiers.</item>
///   <item><b>Too close</b> (inside optimal and below <see cref="VelocityModifiers.TooCloseDistanceM"/>):
///         fly at max speed to disengage.</item>
///   <item><b>Inside optimal range</b>: match target speed with
///         <see cref="VelocityModifiers.InsideOptimalRange"/> modifiers.</item>
/// </list>
///
/// Within each zone, the base velocity is the target's speed (clamped to [MinVelocity, MaxVelocity]),
/// with a fallback when the target is nearly stationary. A directional modifier
/// (<see cref="ModifierByDotProduct"/>) is then applied based on whether the NPC and target
/// are heading in the same direction (positive dot product) or opposite directions (negative).
/// </remarks>
public static class VelocityGoalCalculator
{
    /// <summary>
    /// Input parameters for a single velocity goal calculation.
    /// </summary>
    /// <remarks>
    /// Assembled by <see cref="MovementSimulator"/> from <see cref="MovementInput"/> fields
    /// and the computed braking distance. All distances are in metres; all velocities in m/s.
    /// </remarks>
    public class VelocityGoalInput
    {
        /// <summary>
        /// Distance from the NPC to the target move position, in metres.
        /// </summary>
        /// <remarks>
        /// This is the distance to where the NPC is steering (the move position), which may differ
        /// from <see cref="TargetDistance"/> (distance to the actual target construct).
        /// Used to determine if the NPC is "far away" or within braking range.
        /// Corresponds to <c>TargetMoveDistance</c> in <see cref="MovementInput"/>.
        /// </remarks>
        public required double Distance { get; init; }

        /// <summary>
        /// Distance from the NPC to the actual enemy target construct, in metres.
        /// </summary>
        /// <remarks>
        /// Compared against <see cref="WeaponOptimalRange"/> to determine the range bracket
        /// (inside optimal, outside optimal, outside 2x optimal). This is the combat-relevant
        /// distance, not the navigation distance.
        /// Corresponds to <c>MovementInput.TargetDistance</c>.
        /// </remarks>
        public required double TargetDistance { get; init; }

        /// <summary>
        /// Current linear velocity of the target construct, in m/s.
        /// </summary>
        /// <remarks>
        /// The magnitude (<c>Size()</c>) serves as the base velocity goal in most range zones --
        /// the NPC tries to match the target's speed. When the target is nearly stationary
        /// (magnitude &lt; <see cref="MinVelocity"/>), fallback formulas are used instead.
        /// The normalized direction is dotted with the NPC's velocity to select the positive
        /// or negative modifier branch.
        /// </remarks>
        public required Vec3 TargetLinearVelocity { get; init; }

        /// <summary>
        /// Current linear velocity of the NPC construct, in m/s.
        /// </summary>
        /// <remarks>
        /// Used only for its direction (normalized). The dot product of the NPC's velocity
        /// direction with the target's velocity direction determines which
        /// <see cref="ModifierByDotProduct"/> multiplier (Positive or Negative) is applied.
        /// </remarks>
        public required Vec3 NpcVelocity { get; init; }

        /// <summary>
        /// Minimum allowed velocity goal, in m/s.
        /// </summary>
        /// <remarks>
        /// Acts as the floor when clamping the target's speed to produce a base velocity.
        /// Also used as the velocity goal inside optimal range when the target is nearly stationary.
        /// Derived from <see cref="MovementInput.MinSpeedKph"/> via <c>MinSpeedKph / 3.6</c>.
        /// </remarks>
        public required double MinVelocity { get; init; }

        /// <summary>
        /// Maximum allowed velocity goal, in m/s.
        /// </summary>
        /// <remarks>
        /// Acts as the ceiling when clamping the target's speed and as the velocity goal
        /// for "far" and "too close" zones. Also used in fallback formulas as the dividend
        /// for the alpha divisors.
        /// Derived from <see cref="MovementInput.MaxSpeedKph"/> via <c>MaxSpeedKph / 3.6</c>.
        /// </remarks>
        public required double MaxVelocity { get; init; }

        /// <summary>
        /// Optimal firing range of the NPC's best weapon, in metres.
        /// </summary>
        /// <remarks>
        /// Defines the range bracket boundaries: <c>WeaponOptimalRange</c> separates "inside"
        /// from "outside" optimal range, and <c>WeaponOptimalRange * 2</c> separates "outside"
        /// from "outside 2x" optimal range.
        /// Computed from <c>BehaviorContext.GetBestWeaponOptimalRange()</c> in the original code.
        /// </remarks>
        public required double WeaponOptimalRange { get; init; }

        /// <summary>
        /// Velocity modifier configuration controlling per-zone speed scaling.
        /// </summary>
        /// <remarks>
        /// See <see cref="VelocityModifiers"/> for details on each modifier.
        /// When <see cref="VelocityModifiers.Enabled"/> is <c>false</c>, the calculator
        /// short-circuits and returns <see cref="MaxVelocity"/>.
        /// </remarks>
        public required VelocityModifiers Modifiers { get; init; }

        /// <summary>
        /// Whether the NPC is navigating to a manually-specified override position (e.g., a waypoint).
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, the calculator uses a simplified two-state approach:
        /// either max speed or full stop, based on whether the NPC is within braking distance.
        /// See <see cref="OverrideMovePositionDistance"/>.
        /// </remarks>
        public required bool HasOverrideTargetMovePosition { get; init; }

        /// <summary>
        /// Distance from the NPC to the override target move position, in metres.
        /// </summary>
        /// <remarks>
        /// Only meaningful when <see cref="HasOverrideTargetMovePosition"/> is <c>true</c>.
        /// Compared against <c><see cref="BrakingDistance"/> * <see cref="VelocityModifiers.BrakeDistanceFactor"/></c>
        /// to decide whether to brake to a stop.
        /// </remarks>
        public required double OverrideMovePositionDistance { get; init; }

        /// <summary>
        /// Estimated distance needed to decelerate from current speed to zero, in metres.
        /// </summary>
        /// <remarks>
        /// Computed by <see cref="VelocityHelper.CalculateBrakingDistance"/> using the NPC's current
        /// speed and its acceleration (via <see cref="MovementInput.GetAccelerationMps"/>).
        /// Multiplied by <see cref="VelocityModifiers.BrakeDistanceFactor"/> to add a safety margin
        /// when deciding whether to decelerate or fly at full speed.
        /// </remarks>
        public required double BrakingDistance { get; init; }
    }

    /// <summary>
    /// Computes the velocity goal for the current tick based on the provided input.
    /// </summary>
    /// <param name="input">
    /// All parameters needed for the calculation, including distances, velocities,
    /// weapon range, and modifier configuration.
    /// </param>
    /// <returns>
    /// The desired speed in m/s that the movement strategy should target.
    /// The movement strategy will accelerate or decelerate toward this value.
    /// </returns>
    /// <remarks>
    /// Evaluation order:
    /// <list type="number">
    ///   <item>If modifiers are disabled, return max velocity.</item>
    ///   <item>If an override move position is active, use simplified brake-or-go logic.</item>
    ///   <item>If beyond the far distance threshold, return max velocity.</item>
    ///   <item>If outside 2x optimal range or beyond braking distance, apply "outside 2x" rules.</item>
    ///   <item>If outside optimal range, apply "outside" rules.</item>
    ///   <item>If closer than <see cref="VelocityModifiers.TooCloseDistanceM"/>, return max velocity.</item>
    ///   <item>Otherwise (inside optimal range), apply "inside" rules.</item>
    /// </list>
    /// </remarks>
    public static double Calculate(VelocityGoalInput input)
    {
        if (!input.Modifiers.Enabled) return input.MaxVelocity;
        if (input.HasOverrideTargetMovePosition) return CalculateOverrideMoveVelocityGoal(input);

        var npcDirection = input.NpcVelocity.NormalizeSafe();
        var targetDirection = input.TargetLinearVelocity.NormalizeSafe();
        var velocityWithTargetDotProduct = npcDirection.Dot(targetDirection);
        var oppositeVector = velocityWithTargetDotProduct < 0;

        if (input.Distance > input.Modifiers.GetFarDistanceM())
        {
            return input.MaxVelocity;
        }

        var isOutsideDoubleOptimalRange = input.TargetDistance > input.WeaponOptimalRange * 2;
        var isOutsideOptimalRange = input.TargetDistance > input.WeaponOptimalRange;

        if (isOutsideDoubleOptimalRange || input.Distance > input.BrakingDistance * input.Modifiers.BrakeDistanceFactor)
        {
            var baseVelocity = GetOutsideOfOptimalRange2XTargetVelocity(input);
            return oppositeVector
                ? baseVelocity * input.Modifiers.OutsideOptimalRange2X.Negative
                : baseVelocity * input.Modifiers.OutsideOptimalRange2X.Positive;
        }

        if (isOutsideOptimalRange)
        {
            var baseVelocity = GetOutsideOfOptimalRangeTargetVelocity(input);
            return oppositeVector
                ? baseVelocity * input.Modifiers.OutsideOptimalRange.Negative
                : baseVelocity * input.Modifiers.OutsideOptimalRange.Positive;
        }

        if (input.Distance < input.Modifiers.TooCloseDistanceM)
        {
            return input.MaxVelocity;
        }

        var insideVelocity = GetInsideOfOptimalRangeTargetVelocity(input);
        return oppositeVector
            ? insideVelocity * input.Modifiers.InsideOptimalRange.Negative
            : insideVelocity * input.Modifiers.InsideOptimalRange.Positive;
    }

    /// <summary>
    /// Simplified velocity goal for override-move-position mode (e.g., waypoint navigation).
    /// Returns 0 (full stop) if within braking distance, otherwise max velocity.
    /// </summary>
    private static double CalculateOverrideMoveVelocityGoal(VelocityGoalInput input)
    {
        if (input.OverrideMovePositionDistance <= input.BrakingDistance * input.Modifiers.BrakeDistanceFactor)
        {
            return 0d;
        }

        return input.MaxVelocity;
    }

    /// <summary>
    /// Computes the base velocity when the NPC is outside 2x the weapon's optimal range.
    /// If the target is nearly stationary, returns <c>MaxVelocity / OutsideOptimalRange2XAlpha</c>;
    /// otherwise returns the target's speed clamped to [MinVelocity, MaxVelocity].
    /// </summary>
    private static double GetOutsideOfOptimalRange2XTargetVelocity(VelocityGoalInput input)
    {
        if (input.TargetLinearVelocity.Size() < input.MinVelocity)
        {
            return input.MaxVelocity / input.Modifiers.OutsideOptimalRange2XAlpha;
        }

        return System.Math.Clamp(input.TargetLinearVelocity.Size(), input.MinVelocity, input.MaxVelocity);
    }

    /// <summary>
    /// Computes the base velocity when the NPC is between 1x and 2x the weapon's optimal range.
    /// If the target is nearly stationary, returns <c>MaxVelocity / OutsideOptimalRangeAlpha</c>;
    /// otherwise returns the target's speed clamped to [MinVelocity, MaxVelocity].
    /// </summary>
    private static double GetOutsideOfOptimalRangeTargetVelocity(VelocityGoalInput input)
    {
        if (input.TargetLinearVelocity.Size() < input.MinVelocity)
        {
            return input.MaxVelocity / input.Modifiers.OutsideOptimalRangeAlpha;
        }

        return System.Math.Clamp(input.TargetLinearVelocity.Size(), input.MinVelocity, input.MaxVelocity);
    }

    /// <summary>
    /// Computes the base velocity when the NPC is inside the weapon's optimal range
    /// (and beyond <see cref="VelocityModifiers.TooCloseDistanceM"/>).
    /// If the target is nearly stationary, returns <see cref="VelocityGoalInput.MinVelocity"/>;
    /// otherwise returns the target's speed clamped to [MinVelocity, MaxVelocity].
    /// </summary>
    private static double GetInsideOfOptimalRangeTargetVelocity(VelocityGoalInput input)
    {
        if (input.TargetLinearVelocity.Size() < input.MinVelocity)
        {
            return input.MinVelocity;
        }

        return System.Math.Clamp(input.TargetLinearVelocity.Size(), input.MinVelocity, input.MaxVelocity);
    }
}
