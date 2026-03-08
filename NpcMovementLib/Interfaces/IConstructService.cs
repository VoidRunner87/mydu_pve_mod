using NpcMovementLib.Data;

namespace NpcMovementLib.Interfaces;

/// <summary>
/// Provides access to construct transform and velocity data.
/// </summary>
public interface IConstructService
{
    /// <summary>
    /// Gets the position and rotation of a construct.
    /// </summary>
    Task<ConstructTransformResult> GetConstructTransformAsync(ulong constructId);

    /// <summary>
    /// Gets the linear and angular velocities of a construct.
    /// </summary>
    Task<ConstructVelocityResult> GetConstructVelocities(ulong constructId);

    /// <summary>
    /// Checks whether a construct exists.
    /// </summary>
    Task<bool> Exists(ulong constructId);
}
