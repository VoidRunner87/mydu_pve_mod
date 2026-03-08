using NpcMovementLib.Data;
using NpcMovementLib.Math;

namespace NpcMovementLib.Interfaces;

/// <summary>
/// Provides radar scanning capability to detect player constructs in range.
/// </summary>
public interface IRadarService
{
    /// <summary>
    /// Scans for player contacts around a position within a given radius.
    /// </summary>
    /// <param name="constructId">The NPC construct performing the scan</param>
    /// <param name="position">The scan origin position</param>
    /// <param name="radius">The scan radius in meters</param>
    /// <returns>List of detected contacts</returns>
    Task<IList<ScanContact>> ScanForPlayerContacts(ulong constructId, Vec3 position, double radius);
}
