using NpcMovementLib.Math;

namespace NpcMovementLib.Data;

/// <summary>
/// Result containing the linear and angular velocity of a construct,
/// typically retrieved from the game server for a target construct.
/// </summary>
/// <remarks>
/// Used to obtain the target's current velocity so the NPC can match speed
/// or predict the target's future position. In the original code, the target's linear velocity
/// is stored in <c>BehaviorContext.TargetLinearVelocity</c> and fed into
/// <see cref="VelocityGoalCalculator"/> to compute range-appropriate speed goals.
/// </remarks>
public class ConstructVelocityResult
{
    /// <summary>
    /// Linear (translational) velocity of the construct, in m/s.
    /// </summary>
    /// <remarks>
    /// Used as <see cref="MovementInput.TargetLinearVelocity"/> when this result describes the
    /// enemy target. The magnitude (<c>Size()</c>) is compared against
    /// <see cref="MovementInput.MinVelocity"/> to decide whether the target is
    /// "nearly stationary" (triggering fallback speed calculations in <see cref="VelocityGoalCalculator"/>).
    /// </remarks>
    public Vec3 Linear { get; set; }

    /// <summary>
    /// Angular (rotational) velocity of the construct, in radians/s per axis.
    /// </summary>
    /// <remarks>
    /// Not currently consumed by the NPC movement system but included for completeness.
    /// Could be used in the future for predicting target rotation or for applying angular
    /// velocity to the NPC construct itself.
    /// </remarks>
    public Vec3 Angular { get; set; }
}
