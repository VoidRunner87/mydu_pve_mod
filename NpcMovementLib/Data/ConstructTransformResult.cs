using System.Numerics;
using NpcMovementLib.Math;

namespace NpcMovementLib.Data;

/// <summary>
/// Result returned by <see cref="Interfaces.IConstructService.GetConstructTransformAsync"/>
/// containing the current world-space transform of a construct.
/// </summary>
/// <remarks>
/// If the construct does not exist (e.g., it was destroyed), <see cref="ConstructExists"/>
/// will be <c>false</c> and the other fields should be ignored.
/// In the original code, this data was retrieved inline in <c>FollowTargetBehaviorV2.TickAsync</c>
/// via the construct service and written into <c>BehaviorContext.Position</c> / <c>Rotation</c>.
/// </remarks>
public class ConstructTransformResult
{
    /// <summary>
    /// Whether the queried construct still exists in the game world.
    /// </summary>
    /// <remarks>
    /// When <c>false</c>, the NPC should skip its movement tick because the construct is gone
    /// (e.g., destroyed or despawned). The <see cref="Position"/> and <see cref="Rotation"/>
    /// fields are meaningless in this case.
    /// </remarks>
    public bool ConstructExists { get; set; }

    /// <summary>
    /// World-space position of the construct, in metres.
    /// </summary>
    /// <remarks>
    /// Only valid when <see cref="ConstructExists"/> is <c>true</c>.
    /// This is the authoritative server-side position used to initialize or correct
    /// the NPC's <see cref="MovementInput.Position"/> on the first tick or after reconnection.
    /// </remarks>
    public Vec3 Position { get; set; }

    /// <summary>
    /// Orientation of the construct as a unit quaternion.
    /// </summary>
    /// <remarks>
    /// Only valid when <see cref="ConstructExists"/> is <c>true</c>.
    /// Defaults to <see cref="Quaternion.Identity"/> (no rotation).
    /// Used to seed <see cref="MovementInput.Rotation"/> when the movement loop starts.
    /// </remarks>
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
}
