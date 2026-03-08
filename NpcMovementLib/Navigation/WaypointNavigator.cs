using NpcMovementLib.Math;

namespace NpcMovementLib.Navigation;

public class WaypointNavigator
{
    private readonly double _arrivalDistance;
    private readonly IReadOnlyList<Vec3> _originalWaypoints;

    public Queue<Vec3> WaypointQueue { get; private set; }
    public bool ResetOnCompletion { get; set; }
    public bool HasArrived => WaypointQueue.Count == 0;

    public WaypointNavigator(IEnumerable<Vec3> waypoints, double arrivalDistance = 50000)
    {
        _arrivalDistance = arrivalDistance;
        _originalWaypoints = waypoints.ToList();
        WaypointQueue = new Queue<Vec3>(_originalWaypoints);
    }

    public Vec3? GetCurrentTarget(Vec3 currentPosition)
    {
        if (WaypointQueue.Count == 0)
        {
            if (ResetOnCompletion)
            {
                WaypointQueue = new Queue<Vec3>(_originalWaypoints);
            }
            else
            {
                return null;
            }
        }

        if (WaypointQueue.Count == 0) return null;

        var nextWaypoint = WaypointQueue.Peek();

        if (currentPosition.Dist(nextWaypoint) < _arrivalDistance)
        {
            WaypointQueue.Dequeue();

            if (WaypointQueue.Count == 0)
            {
                if (ResetOnCompletion)
                {
                    WaypointQueue = new Queue<Vec3>(_originalWaypoints);
                }

                return WaypointQueue.Count > 0 ? WaypointQueue.Peek() : null;
            }

            nextWaypoint = WaypointQueue.Peek();
        }

        return nextWaypoint;
    }

    public void Reset()
    {
        WaypointQueue = new Queue<Vec3>(_originalWaypoints);
    }
}
