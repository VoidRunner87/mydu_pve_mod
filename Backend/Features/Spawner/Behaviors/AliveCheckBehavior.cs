using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Interfaces;
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
    private IConstructInfoGrain _constructInfoGrain;
    private ElementId _coreUnitElementId;
    
    private bool _active = true;
    private IConstructHandleRepository _handleRepository;

    public bool IsActive() => _active;

    public async Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _orleans = provider.GetOrleans();

        _handleRepository = provider.GetRequiredService<IConstructHandleRepository>();
        _constructInfoGrain = _orleans.GetConstructInfoGrain(constructId);
        _constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);
        _coreUnitElementId = (await _constructElementsGrain.GetElementsOfType<CoreUnit>()).SingleOrDefault();
        
        context.IsAlive = _coreUnitElementId.elementId > 0;
        _active = context.IsAlive;
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            _active = false;

            await _handleRepository.RemoveHandleAsync(constructId);
            
            return;
        }
        
        var coreUnit = await _constructElementsGrain.GetElement(_coreUnitElementId);
        var constructInfo = await _constructInfoGrain.Get();

        if (coreUnit.IsCoreDestroyed() || constructInfo.IsAbandoned())
        {
            await context.NotifyConstructDestroyedAsync(new BehaviorEventArgs(constructId, prefab, context));
            _active = false;
            context.IsAlive = false;
            
            await _handleRepository.RemoveHandleAsync(constructId);
        }
    }
}