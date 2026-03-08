using NpcMovementLib.Data;

namespace NpcMovementLib.Interfaces;

/// <summary>
/// Provides read-only access to a construct's transform (position + rotation) and velocity data.
/// </summary>
/// <remarks>
/// <para>
/// This is the library-side abstraction of the game backend's
/// <c>Mod.DynamicEncounters.Features.Common.Interfaces.IConstructService</c>. The backend
/// implementation (<c>ConstructService</c>) retrieves data from Orleans grains
/// (<c>GetConstructInfoGrain</c>, <c>GetConstructGrain</c>) and falls back to direct
/// PostgreSQL queries when grains are unavailable.
/// </para>
/// <para>
/// In the NPC behavior pipeline, <see cref="GetConstructTransformAsync"/> is called at the start
/// of the first movement tick to initialize the NPC's position when <c>BehaviorContext.Position</c>
/// has no value. <see cref="GetConstructVelocities"/> is used to read the current linear and angular
/// velocities as absolute world-space values in metres per second (m/s). These velocities are
/// tick-rate-independent and must be multiplied by <c>deltaTime</c> to obtain per-tick displacements.
/// </para>
/// <para>
/// <see cref="Exists"/> is used during cleanup and alive-check behaviors to verify that a construct
/// has not been deleted before continuing to process it.
/// </para>
/// <para>
/// Implementations may apply caching. The backend's <c>CachedConstructService</c> wraps the base
/// service with an in-memory cache; callers that need fresh data should bypass the cache layer.
/// </para>
/// </remarks>
public interface IConstructService
{
    /// <summary>
    /// Gets the world-space position and rotation of the specified construct.
    /// </summary>
    /// <param name="constructId">
    /// The strongly-typed identifier of the construct (NPC or player).
    /// See <see cref="ConstructId"/> for conversion semantics.
    /// </param>
    /// <returns>
    /// A <see cref="ConstructTransformResult"/> containing:
    /// <list type="bullet">
    ///   <item><see cref="ConstructTransformResult.ConstructExists"/> -- <see langword="false"/>
    ///         if the construct was not found or has been deleted.</item>
    ///   <item><see cref="ConstructTransformResult.Position"/> -- the construct's absolute
    ///         world-space position in metres.</item>
    ///   <item><see cref="ConstructTransformResult.Rotation"/> -- the construct's orientation
    ///         as a <see cref="System.Numerics.Quaternion"/>.</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// In the game backend, this first attempts to read from the Orleans <c>ConstructInfoGrain</c>.
    /// If the grain call fails but the construct still exists in the database, it falls back to a
    /// direct SQL query against <c>public.construct</c>.
    /// </remarks>
    Task<ConstructTransformResult> GetConstructTransformAsync(ConstructId constructId);

    /// <summary>
    /// Gets the linear and angular velocities of the specified construct.
    /// </summary>
    /// <param name="constructId">
    /// The strongly-typed identifier of the construct whose velocities are being queried.
    /// </param>
    /// <returns>
    /// A <see cref="ConstructVelocityResult"/> containing:
    /// <list type="bullet">
    ///   <item><see cref="ConstructVelocityResult.Linear"/> -- the construct's absolute
    ///         linear velocity in metres per second (m/s). This is <b>not</b> a per-frame
    ///         displacement; multiply by <c>deltaTime</c> to get per-tick displacement.</item>
    ///   <item><see cref="ConstructVelocityResult.Angular"/> -- the construct's angular
    ///         velocity in radians per second.</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// In the game backend, this calls <c>GetConstructGrain(constructId).GetConstructVelocity()</c>
    /// on the Orleans cluster. The returned velocity is used both for movement simulation (via
    /// <see cref="MovementSimulator"/>) and for relative-velocity calculations during combat
    /// (e.g., weapon lead prediction: <c>futurePos = pos + velocity * t + 0.5 * accel * t^2</c>).
    /// </remarks>
    Task<ConstructVelocityResult> GetConstructVelocities(ConstructId constructId);

    /// <summary>
    /// Checks whether a construct with the given identifier exists in the game world.
    /// </summary>
    /// <param name="constructId">
    /// The strongly-typed identifier of the construct to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a construct record exists (regardless of deletion state);
    /// <see langword="false"/> otherwise.
    /// </returns>
    /// <remarks>
    /// In the game backend, this executes a <c>SELECT COUNT(0) FROM public.construct WHERE id = @constructId</c>
    /// query. Note that this checks for existence only -- it does not verify whether the construct
    /// has been soft-deleted. Use the backend's <c>ExistsAndNotDeleted</c> method when deletion
    /// state matters (e.g., alive-check behaviors).
    /// </remarks>
    Task<bool> Exists(ConstructId constructId);
}
