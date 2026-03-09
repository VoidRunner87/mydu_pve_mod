using NpcCommonLib.Data;
using NpcCommonLib.Math;

namespace NpcWeaponLib.Interfaces;

/// <summary>
/// Determines where on a target construct a shot will impact.
/// </summary>
/// <remarks>
/// In the game backend, this queries the voxel service (<c>IVoxelServiceClient.QueryRandomPoint</c>)
/// to find a valid surface point on the target. If the voxel service is unavailable,
/// a random direction scaled by construct size is used as fallback.
/// </remarks>
public interface IHitPositionService
{
    /// <summary>
    /// Returns a local-space hit position on the target construct.
    /// </summary>
    /// <param name="targetConstructId">Target to query.</param>
    /// <param name="shooterPosition">Shooter's world position (used to determine facing).</param>
    /// <returns>Hit position in target's local coordinate space.</returns>
    Task<Vec3> GetHitPositionAsync(ConstructId targetConstructId, Vec3 shooterPosition);
}
