using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class WaypointMoveBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private readonly IPrefab _prefab = prefab;
    private IConstructService _constructService;
    private ILogger<WaypointMoveBehavior> _logger;
    private IConstructHandleRepository _constructHandleService;

    public BehaviorTaskCategory Category => BehaviorTaskCategory.HighPriority;

    public async Task InitializeAsync(BehaviorContext context)
    {
        await Task.Yield();

        var provider = context.ServiceProvider;

        _constructService = provider.GetRequiredService<IConstructService>();
        _constructHandleService = provider.GetRequiredService<IConstructHandleRepository>();
        _logger = provider.CreateLogger<WaypointMoveBehavior>();
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            return;
        }

        if (!context.IsWaypointListInitialized())
        {
            var unparsedWaypointList = context.GetUnparsedWaypointList();
            var waypointList = ParseWaypointList(unparsedWaypointList);
            context.SetWaypointList(waypointList);
            context.TagWaypointListInitialized();
        }

        var targetWaypoint = context.GetNextNotVisited();
        if (targetWaypoint != null)
        {
            context.SetAutoTargetMovePosition(targetWaypoint.Position);
            context.SetMoveModeWaypoint();

            if (context.Position.HasValue)
            {
                var distance = (targetWaypoint.Position! - context.Position.Value).Size();

                _logger.LogDebug(
                    "Construct {Construct} Navigates to Waypoint {TargetWaypoint}. Distance {Distance}",
                    constructId,
                    targetWaypoint.Position,
                    Math.Round(distance / DistanceHelpers.OneSuInMeters)
                );
            }
        }
        
        var npcTransformOutcome = await _constructService.GetConstructTransformAsync(constructId);
        if (!npcTransformOutcome.ConstructExists)
        {
            return;
        }
        var npcPos = npcTransformOutcome.Position;

        // Arrived Near Destination
        if (context.GetTargetMovePosition().Dist(npcPos) <= 50000 && targetWaypoint != null)
        {
            targetWaypoint.Visited = true;
        }

        // almost done braking, can move on
        if (context.IsBraking() && context.Velocity.Size() < 1000)
        {
            context.SetBraking(false);
        }
    }

    private IEnumerable<Waypoint> ParseWaypointList(object obj)
    {
        try
        {
            return JArray.FromObject(obj).ToObject<List<Waypoint>>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to parse Waypoint List. Assume Empty");
            return new List<Waypoint>();
        }
    }

    private async Task Despawn()
    {
        await _constructHandleService.RemoveHandleAsync(constructId);
        await _constructService.SoftDeleteAsync(constructId);
        _logger.LogInformation("Despawned Construct {Construct}", constructId);
    }
}