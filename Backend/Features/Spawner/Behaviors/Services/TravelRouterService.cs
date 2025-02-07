using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Services;

public class TravelRouterService : ITravelRouteService
{
    public IEnumerable<WaypointItem> Solve(WaypointItem initialPosition, IEnumerable<WaypointItem> positions)
    {
        var route = new List<WaypointItem>();
        var unvisited = new List<WaypointItem>(positions);
        var current = initialPosition;

        route.Add(new WaypointItem
        {
            Name = current.Name,
            Position = current.Position
        });
        unvisited.Remove(current);

        var counter = 1;
        while (unvisited.Count > 0)
        {
            var nearestIndex = FindNearestNeighbor(current, unvisited);
            current = unvisited[nearestIndex];

            route.Add(new WaypointItem
            {
                Name = $"{current.Name}_{counter}",
                Position = current.Position
            });
            unvisited.RemoveAt(nearestIndex);

            counter++;
        }

        return route;
    }

    private static int FindNearestNeighbor(WaypointItem current, IReadOnlyList<WaypointItem> unvisited)
    {
        var minDistance = double.MaxValue;
        var nearestIndex = -1;

        for (var i = 0; i < unvisited.Count; i++)
        {
            var distance = Distance(current, unvisited[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private static double Distance(WaypointItem a, WaypointItem b)
    {
        return a.Position.Dist(b.Position);
    }
}