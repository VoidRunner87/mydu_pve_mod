using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Helpers.DU;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class AliveCheckBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private IClusterClient _orleans;
    private IConstructElementsGrain _constructElementsGrain;
    private ElementId _coreUnitElementId;
    
    private IConstructHandleRepository _handleRepository;
    private IConstructService _constructService;

    public async Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _orleans = provider.GetOrleans();

        _handleRepository = provider.GetRequiredService<IConstructHandleRepository>();
        _constructService = provider.GetRequiredService<IConstructService>();
        _constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);
        _coreUnitElementId = (await _constructElementsGrain.GetElementsOfType<CoreUnit>()).SingleOrDefault();
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            await _handleRepository.RemoveHandleAsync(constructId);
            
            return;
        }
        
        if (!context.IsBehaviorActive<AliveCheckBehavior>())
        {
            return;
        }
        
        var coreUnit = await _constructElementsGrain.GetElement(_coreUnitElementId);
        var constructInfo = await _constructService.GetConstructInfoAsync(constructId);
        if (constructInfo == null)
        {
            return;
        }

        if (coreUnit.IsCoreDestroyed() || constructInfo.IsAbandoned())
        {
            await context.NotifyConstructDestroyedAsync(new BehaviorEventArgs(constructId, prefab, context));
            context.Deactivate<AliveCheckBehavior>();
            context.IsAlive = false;
            
            await _handleRepository.RemoveHandleAsync(constructId);
        }
    }
}