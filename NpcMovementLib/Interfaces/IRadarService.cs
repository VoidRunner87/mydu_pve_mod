using NpcMovementLib.Data;
using NpcMovementLib.Math;

namespace NpcMovementLib.Interfaces;

/// <summary>
/// Provides radar scanning capability to detect player-owned constructs within a specified range
/// of an NPC construct.
/// </summary>
/// <remarks>
/// <para>
/// This is the library-side abstraction of the game backend's <c>IAreaScanService</c>
/// (implemented by <c>AreaScanService</c> and its cached wrapper <c>CachedAreaScanService</c>).
/// The backend implementation executes a spatial SQL query against the <c>public.construct</c>
/// table using PostGIS 3D distance functions (<c>ST_3DDistance</c>, <c>ST_DWithin</c>) to find
/// nearby player constructs.
/// </para>
/// <para>
/// The scan filters out:
/// <list type="bullet">
///   <item>NPC-owned constructs (those present in the <c>mod_npc_construct_handle</c> table).</item>
///   <item>Deleted constructs (<c>deleted_at IS NOT NULL</c>).</item>
///   <item>Untargetable constructs (<c>isUntargetable = true</c>).</item>
///   <item>Non-dynamic constructs (only kinds 4 and 5 -- dynamic and space constructs).</item>
///   <item>Constructs owned by the NQ bot player (player_id = 4).</item>
///   <item>The scanning NPC's own construct.</item>
/// </list>
/// </para>
/// <para>
/// In the NPC behavior pipeline, radar scanning is used by the <c>SelectTargetBehavior</c>
/// (running at MediumPriority, ~1 FPS) to choose which player construct the NPC should pursue
/// or attack. The resulting <see cref="ScanContact"/> list is sorted by ascending distance.
/// </para>
/// </remarks>
public interface IRadarService
{
    /// <summary>
    /// Scans for player-owned constructs around a position within the specified radius.
    /// </summary>
    /// <param name="constructId">
    /// The strongly-typed identifier of the NPC construct performing the scan. This construct
    /// is excluded from the results to prevent the NPC from detecting itself.
    /// See <see cref="ConstructId"/> for conversion semantics.
    /// </param>
    /// <param name="position">
    /// The scan origin in absolute world-space coordinates (metres). Typically the NPC's
    /// current position from <see cref="IConstructService.GetConstructTransformAsync"/> or
    /// the locally tracked position in the behavior context.
    /// </param>
    /// <param name="radius">
    /// The scan radius in metres. Only constructs whose 3D distance from
    /// <paramref name="position"/> is less than or equal to this value are returned.
    /// Typical values range from 100,000 m (0.5 SU) for long-range detection to smaller
    /// values for close-range engagement scans.
    /// </param>
    /// <returns>
    /// A list of <see cref="ScanContact"/> instances representing detected player constructs,
    /// sorted by ascending distance from the scan origin. Each contact includes:
    /// <list type="bullet">
    ///   <item><see cref="ScanContact.ConstructId"/> -- the detected construct's unique ID.</item>
    ///   <item><see cref="ScanContact.Name"/> -- the construct's display name.</item>
    ///   <item><see cref="ScanContact.Distance"/> -- the 3D distance in metres from the scan origin.</item>
    ///   <item><see cref="ScanContact.Position"/> -- the detected construct's world-space position.</item>
    /// </list>
    /// The list may be empty if no player constructs are within range.
    /// </returns>
    /// <remarks>
    /// In the game backend, the underlying SQL query uses PostGIS spatial indexes for
    /// efficient range queries. The <c>CachedAreaScanService</c> wrapper may cache results
    /// briefly to reduce database load when multiple NPC behaviors scan in the same tick.
    /// </remarks>
    Task<IList<ScanContact>> ScanForPlayerContacts(ConstructId constructId, Vec3 position, double radius);
}
