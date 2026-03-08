using NpcMovementLib.Math;

namespace NpcMovementLib.Data;

/// <summary>
/// Represents a single radar contact detected by the NPC's scanning system.
/// </summary>
/// <remarks>
/// In the original game code, radar contacts are collected in <c>BehaviorContext.Contacts</c>
/// (a <c>ConcurrentBag&lt;ScanContact&gt;</c>) and used by target-selection behaviours
/// such as <c>SelectTargetBehavior</c> to pick the closest or highest-threat target.
/// This is the NpcMovementLib equivalent, returned by <see cref="Interfaces.IRadarService"/>.
/// </remarks>
public class ScanContact
{
    /// <summary>
    /// Unique identifier of the detected construct.
    /// </summary>
    /// <remarks>
    /// Used to look up further information about the contact (e.g., velocity, damage data)
    /// and to set the NPC's target via <c>BehaviorContext.SetTargetConstructId</c> in the
    /// original code.
    /// </remarks>
    public required ulong ConstructId { get; set; }

    /// <summary>
    /// Display name of the detected construct (e.g., the ship's pretty name).
    /// </summary>
    /// <remarks>
    /// Primarily used for logging and debugging. Corresponds to the construct's
    /// header pretty name from the game's construct info.
    /// </remarks>
    public required string Name { get; set; }

    /// <summary>
    /// Distance from the NPC to this contact, in metres.
    /// </summary>
    /// <remarks>
    /// Used for target selection -- the contact with the smallest distance is returned
    /// by <c>BehaviorContext.GetClosestTarget()</c> in the original code.
    /// Also used to filter contacts within weapon range.
    /// </remarks>
    public required double Distance { get; set; }

    /// <summary>
    /// World-space position of the detected construct, in metres.
    /// </summary>
    /// <remarks>
    /// Can be used to compute relative vectors, intercept courses, or to set
    /// the NPC's target position for movement calculations.
    /// </remarks>
    public required Vec3 Position { get; set; }
}
