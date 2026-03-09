using NpcCommonLib.Math;

namespace NpcMovementLib.Navigation;

/// <summary>
/// Orders an unordered set of waypoints into a short-path route using a nearest-neighbor
/// greedy heuristic, starting from a given origin position.
/// </summary>
/// <remarks>
/// Ported from the Backend's <c>TravelRouterService</c>. The algorithm is a classic
/// nearest-neighbor TSP (Travelling Salesman Problem) heuristic:
/// <list type="number">
///   <item>Start at the given origin position.</item>
///   <item>Find the unvisited point closest to the current position.</item>
///   <item>Move to that point and mark it as visited.</item>
///   <item>Repeat until all points have been visited.</item>
/// </list>
/// <para>
/// This does not guarantee the globally optimal route, but runs in O(n^2) time and
/// produces a reasonable ordering for typical NPC patrol routes with a small number
/// of waypoints. The returned route includes the start position as the first element.
/// </para>
/// <para>
/// The output is intended to be fed into <see cref="WaypointNavigator"/> for sequential
/// navigation.
/// </para>
/// </remarks>
public static class RouteSolver
{
    /// <summary>
    /// Computes a nearest-neighbor route through the given points, beginning at <paramref name="start"/>.
    /// </summary>
    /// <param name="start">
    /// The origin position (typically the NPC's current location) in world-space metres.
    /// This position is included as the first element of the returned route.
    /// </param>
    /// <param name="points">
    /// The unordered set of waypoints to visit, in world-space metres.
    /// Each point will appear exactly once in the output.
    /// </param>
    /// <returns>
    /// An ordered list of positions starting with <paramref name="start"/> followed by
    /// all <paramref name="points"/> arranged in nearest-neighbor order. The list has
    /// <c>points.Count() + 1</c> elements (start + all waypoints).
    /// </returns>
    public static IList<Vec3> Solve(Vec3 start, IEnumerable<Vec3> points)
    {
        var route = new List<Vec3> { start };
        var unvisited = new List<Vec3>(points);

        var current = start;

        while (unvisited.Count > 0)
        {
            var nearestIndex = FindNearestNeighbor(current, unvisited);
            current = unvisited[nearestIndex];
            route.Add(current);
            unvisited.RemoveAt(nearestIndex);
        }

        return route;
    }

    /// <summary>
    /// Finds the index of the point in <paramref name="unvisited"/> that is closest
    /// to <paramref name="current"/> by Euclidean distance.
    /// </summary>
    /// <param name="current">The reference position to measure distances from.</param>
    /// <param name="unvisited">The candidate points to search. Must contain at least one element.</param>
    /// <returns>
    /// The zero-based index into <paramref name="unvisited"/> of the nearest point.
    /// Returns <c>-1</c> if <paramref name="unvisited"/> is empty (should not occur in normal use).
    /// </returns>
    private static int FindNearestNeighbor(Vec3 current, IReadOnlyList<Vec3> unvisited)
    {
        var minDistance = double.MaxValue;
        var nearestIndex = -1;

        for (var i = 0; i < unvisited.Count; i++)
        {
            var distance = current.Dist(unvisited[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }
}
