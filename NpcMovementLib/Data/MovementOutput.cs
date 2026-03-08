using System.Numerics;
using NpcCommonLib.Math;

namespace NpcMovementLib.Data;

/// <summary>
/// The result of a single movement tick produced by <see cref="MovementSimulator"/>.
/// Contains the NPC's new position, velocity, and rotation after applying movement strategy
/// and rotation interpolation.
/// </summary>
/// <remarks>
/// Consumers should feed these values back into the next tick's <see cref="MovementInput"/>
/// and also send them to the game server as a construct update (position, velocity, rotation).
/// In the original code, these fields are written back to <c>BehaviorContext</c> and transmitted
/// via <c>ConstructUpdate</c> in <c>FollowTargetBehaviorV2</c>.
/// </remarks>
public class MovementOutput
{
    /// <summary>
    /// New world-space position of the NPC construct after this tick, in metres.
    /// </summary>
    /// <remarks>
    /// Computed by the active <see cref="Strategies.IMovementStrategy"/> (e.g.,
    /// <see cref="Strategies.BurnToTargetStrategy"/> or <see cref="Strategies.BrakingStrategy"/>).
    /// Should be stored and sent to the server for the construct transform update.
    /// </remarks>
    public required Vec3 Position { get; init; }

    /// <summary>
    /// New linear velocity of the NPC construct after this tick, in m/s.
    /// </summary>
    /// <remarks>
    /// Updated by the movement strategy accounting for acceleration, delta-V clamping,
    /// and speed limits. Feed this back as <see cref="MovementInput.Velocity"/> for the next tick
    /// and optionally as <see cref="MovementInput.PreviousVelocity"/> for delta-V clamping.
    /// </remarks>
    public required Vec3 Velocity { get; init; }

    /// <summary>
    /// New orientation of the NPC construct as a unit quaternion, after rotation interpolation.
    /// </summary>
    /// <remarks>
    /// Computed via <c>Quaternion.Slerp</c> between the previous rotation and a target rotation
    /// that faces the movement direction. The interpolation speed is governed by
    /// <see cref="MovementInput.RotationSpeed"/> multiplied by <see cref="MovementInput.DeltaTime"/>.
    /// Feed this back as <see cref="MovementInput.Rotation"/> for the next tick.
    /// </remarks>
    public required Quaternion Rotation { get; init; }
}
