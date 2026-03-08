using NpcMovementLib.Math;

namespace NpcMovementLib.Navigation;

public static class RouteSolver
{
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
