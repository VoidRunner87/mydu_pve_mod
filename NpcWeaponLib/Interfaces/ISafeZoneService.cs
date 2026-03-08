using NpcMovementLib.Data;

namespace NpcWeaponLib.Interfaces;

/// <summary>
/// Checks whether a construct is inside a safe zone (PvP-free area).
/// Firing is suppressed when either shooter or target is in a safe zone.
/// </summary>
public interface ISafeZoneService
{
    Task<bool> IsInSafeZoneAsync(ConstructId constructId);
}
