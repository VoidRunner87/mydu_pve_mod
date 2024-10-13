using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using Mod.DynamicEncounters.Helpers;

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

        context.TargetWaypoint = context.Waypoints.FirstOrDefault(x => !x.Visited);
        if (context.TargetWaypoint != null)
        {
            context.SetAutoTargetMovePosition(context.TargetWaypoint.Position);
        }
        
        var npcTransformOutcome = await _constructService.GetConstructTransformAsync(constructId);
        if (!npcTransformOutcome.ConstructExists)
        {
            return;
        }
        var npcPos = npcTransformOutcome.Position;

        // Arrived Near Destination
        if (context.GetTargetMovePosition().Dist(npcPos) <= 50000 && context.TargetWaypoint != null)
        {
            context.TargetWaypoint.Visited = true;
        }
    }

    private async Task Despawn()
    {
        await _constructHandleService.RemoveHandleAsync(constructId);
        await _constructService.SoftDeleteAsync(constructId);
        _logger.LogInformation("Despawned Construct {Construct}", constructId);
    }
}