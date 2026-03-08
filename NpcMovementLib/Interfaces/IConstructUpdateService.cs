using System.Numerics;
using NpcMovementLib.Math;

namespace NpcMovementLib.Interfaces;

/// <summary>
/// Sends construct position/rotation/velocity updates to the game server.
/// </summary>
public interface IConstructUpdateService
{
    /// <summary>
    /// Pushes an NPC construct's updated transform to the server.
    /// </summary>
    Task SendConstructUpdate(
        ulong constructId,
        Vec3 position,
        Quaternion rotation,
        Vec3 velocity
    );
}
