using NpcMovementLib.Data;

namespace NpcWeaponLib.Interfaces;

/// <summary>
/// Checks whether a construct is inside a safe zone (PvP-free area).
/// Firing is suppressed when either shooter or target is in a safe zone.
/// </summary>
public interface ISafeZoneService
{
    /// <summary>
    /// Checks whether the given construct is currently inside a safe zone.
    /// </summary>
    /// <param name="constructId">Construct to check.</param>
    /// <returns><c>true</c> if the construct is inside a safe zone; <c>false</c> otherwise.</returns>
    Task<bool> IsInSafeZoneAsync(ConstructId constructId);
}
