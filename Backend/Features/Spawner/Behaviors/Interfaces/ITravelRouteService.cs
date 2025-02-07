using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

public interface ITravelRouteService
{
    IEnumerable<WaypointItem> Solve(
        WaypointItem initialPosition,
        IEnumerable<WaypointItem> positions
    );
}