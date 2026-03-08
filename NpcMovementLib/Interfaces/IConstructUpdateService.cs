using System.Numerics;
using NpcMovementLib.Math;

namespace NpcMovementLib.Interfaces;

/// <summary>
/// Sends construct position, rotation, and velocity updates to the game server,
/// making NPC movement visible to all connected players.
/// </summary>
/// <remarks>
/// <para>
/// This is the library-side abstraction of the construct update mechanism used by
/// <c>FollowTargetBehaviorV2.TickAsync</c> in the game backend. In the backend, updates are
/// sent by constructing a <c>ConstructUpdate</c> message and calling
/// <c>ModBase.Bot.Req.ConstructUpdate(cUpdate)</c>, which pushes the data through the game's
/// internal networking layer so that all clients and systems see the NPC's new state.
/// </para>
/// <para>
/// This interface is called at the end of each movement tick, after
/// <see cref="MovementSimulator.Tick"/> has computed the new position, velocity, and rotation.
/// The typical flow is:
/// <list type="number">
///   <item><see cref="IConstructService.GetConstructTransformAsync"/> reads the initial position (first tick only).</item>
///   <item><see cref="MovementSimulator.Tick"/> computes the new state.</item>
///   <item><see cref="SendConstructUpdate"/> pushes the result to the server.</item>
/// </list>
/// </para>
/// <para>
/// <b>Velocity convention:</b> The <paramref name="velocity"/> parameter passed to
/// <see cref="SendConstructUpdate"/> should be the <em>display velocity</em>, computed as
/// <c>(newPosition - oldPosition) / deltaTime</c>. This converts the per-tick displacement back
/// into an absolute m/s value that the game engine uses for client-side interpolation and
/// physics prediction.
/// </para>
/// </remarks>
public interface IConstructUpdateService
{
    /// <summary>
    /// Pushes an NPC construct's updated transform and velocity to the game server.
    /// </summary>
    /// <param name="constructId">
    /// The unique identifier of the NPC construct being updated.
    /// </param>
    /// <param name="position">
    /// The construct's new absolute world-space position in metres, as computed by
    /// <see cref="MovementSimulator.Tick"/>.
    /// </param>
    /// <param name="rotation">
    /// The construct's new orientation quaternion, as computed by Slerp interpolation
    /// toward the movement direction in <see cref="MovementSimulator.Tick"/>.
    /// </param>
    /// <param name="velocity">
    /// The construct's display velocity in metres per second (m/s). This should be
    /// <c>(newPosition - oldPosition) / deltaTime</c>, converting the per-tick displacement
    /// back into an absolute velocity for the game engine's interpolation system.
    /// In the backend, this value is assigned to both <c>worldAbsoluteVelocity</c> and
    /// <c>worldRelativeVelocity</c> on the <c>ConstructUpdate</c> message.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the update has been acknowledged by the server.
    /// Implementations should handle transient errors (e.g., <c>InvalidSession</c>) gracefully,
    /// as the backend does by triggering a bot reconnection flow.
    /// </returns>
    /// <remarks>
    /// In the game backend, this call is fire-and-forget in practice: if it fails, the error is
    /// logged but the behavior loop continues. A failed update means the NPC will appear to
    /// "stutter" on clients for one tick but will correct itself on the next successful update.
    /// </remarks>
    Task SendConstructUpdate(
        ulong constructId,
        Vec3 position,
        Quaternion rotation,
        Vec3 velocity
    );
}
