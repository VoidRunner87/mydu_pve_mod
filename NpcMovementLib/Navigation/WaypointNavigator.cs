using NpcMovementLib.Math;

namespace NpcMovementLib.Navigation;

/// <summary>
/// Manages sequential navigation through an ordered list of waypoints, advancing to the
/// next waypoint when the NPC arrives within a configurable distance threshold.
/// </summary>
/// <remarks>
/// This is the library equivalent of the waypoint navigation logic in the Backend's
/// behavior system. It is designed to work with <see cref="RouteSolver"/> which produces
/// an optimally ordered list of waypoints.
/// <para>
/// <b>Usage pattern:</b>
/// Each tick, call <see cref="GetCurrentTarget"/> with the NPC's current position.
/// The navigator returns the next waypoint to steer toward, or <c>null</c> when all
/// waypoints have been visited (unless <see cref="ResetOnCompletion"/> is enabled,
/// in which case the route loops).
/// </para>
/// <para>
/// <b>Arrival detection:</b> A waypoint is considered reached when the NPC's position
/// is within <c>arrivalDistance</c> (default 50,000 metres = 50 km) of it.
/// This generous threshold accounts for the fact that NPCs may not decelerate perfectly
/// to the exact waypoint position, especially when using <see cref="Strategies.BurnToTargetStrategy"/>.
/// </para>
/// </remarks>
public class WaypointNavigator
{
    private readonly double _arrivalDistance;
    private readonly IReadOnlyList<Vec3> _originalWaypoints;

    /// <summary>
    /// The remaining waypoints to visit, in order. The front of the queue is the current target.
    /// </summary>
    public Queue<Vec3> WaypointQueue { get; private set; }

    /// <summary>
    /// When <c>true</c>, the navigator reloads the original waypoint list once all waypoints
    /// have been visited, creating an infinite patrol loop. When <c>false</c> (the default),
    /// navigation ends after the last waypoint and <see cref="HasArrived"/> becomes <c>true</c>.
    /// </summary>
    public bool ResetOnCompletion { get; set; }

    /// <summary>
    /// Returns <c>true</c> when all waypoints have been visited and the queue is empty.
    /// If <see cref="ResetOnCompletion"/> is <c>true</c>, this will only be <c>true</c>
    /// momentarily before the queue is refilled on the next <see cref="GetCurrentTarget"/> call.
    /// </summary>
    public bool HasArrived => WaypointQueue.Count == 0;

    /// <summary>
    /// Creates a new waypoint navigator with the specified route and arrival threshold.
    /// </summary>
    /// <param name="waypoints">
    /// An ordered sequence of world-space positions (in metres) defining
    /// the route. The NPC will visit these in order. Use <see cref="RouteSolver.Solve"/>
    /// to produce an optimized ordering from unordered points.
    /// </param>
    /// <param name="arrivalDistance">
    /// The distance (in metres) at which the NPC is considered to have
    /// arrived at a waypoint and should advance to the next one.
    /// Defaults to <c>50,000</c> metres (50 km).
    /// </param>
    public WaypointNavigator(IEnumerable<Vec3> waypoints, double arrivalDistance = 50000)
    {
        _arrivalDistance = arrivalDistance;
        _originalWaypoints = waypoints.ToList();
        WaypointQueue = new Queue<Vec3>(_originalWaypoints);
    }

    /// <summary>
    /// Returns the current waypoint target for the NPC to steer toward, advancing past
    /// any waypoints that have been reached.
    /// </summary>
    /// <param name="currentPosition">
    /// The NPC's current world-space position, in metres.
    /// </param>
    /// <returns>
    /// The next waypoint position to navigate toward, or <c>null</c> if all waypoints
    /// have been visited and <see cref="ResetOnCompletion"/> is <c>false</c> (or if the
    /// original waypoint list was empty).
    /// When <see cref="ResetOnCompletion"/> is <c>true</c> and the queue is exhausted,
    /// the queue is refilled from the original waypoint list and the first waypoint of
    /// the new loop is returned.
    /// </returns>
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

    /// <summary>
    /// Resets the navigator to the beginning of the original waypoint list, discarding
    /// any progress. The NPC will start navigating from the first waypoint again.
    /// </summary>
    public void Reset()
    {
        WaypointQueue = new Queue<Vec3>(_originalWaypoints);
    }
}
