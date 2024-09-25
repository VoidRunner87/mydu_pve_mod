using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Helpers.DU;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class RetreatBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private readonly IPrefab _prefab = prefab;
    private ILogger<RetreatBehavior> _logger;
    private IClusterClient _orleans;
    private IConstructService _constructService;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _orleans = provider.GetOrleans();
        _constructService = provider.GetRequiredService<IConstructService>();
        _logger = provider.CreateLogger<RetreatBehavior>();

        return Task.CompletedTask;
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            return;
        }
        
        var constructInfoGrain = await _constructService.GetConstructInfoAsync(constructId);

        if (constructInfoGrain == null)
        {
            return;
        }

        var npcPos = constructInfoGrain.rData.position;
        
        if (!context.TargetConstructId.HasValue)
        {
            context.TargetMovePosition = context.Sector;
            return;
        }

        if (constructInfoGrain.IsShieldLowerThan25() || constructInfoGrain.IsShieldDown())
        {
            var targetConstructInfo = await _constructService.GetConstructInfoAsync(context.TargetConstructId.Value);

            if (targetConstructInfo == null)
            {
                context.TargetMovePosition = context.Sector;
                return;
            }

            var targetPos = targetConstructInfo.rData.position;

            var retreatDirection = (npcPos - targetPos).NormalizeSafe();

            context.TargetMovePosition = npcPos + retreatDirection * 10 * 200000; // 10 su
        }
    }
}