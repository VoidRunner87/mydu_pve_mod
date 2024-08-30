using System.Linq;
using System.Threading.Tasks;
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

public class AliveCheckBehavior(ulong constructId, IConstructDefinition constructDefinition) : IConstructBehavior
{
    private IClusterClient _orleans;
    private IConstructElementsGrain _constructElementsGrain;
    private IConstructInfoGrain _constructInfoGrain;
    private ElementId _coreUnitElementId;
    
    private bool _active = true;

    public bool IsActive() => _active;

    public async Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _orleans = provider.GetOrleans();

        _constructInfoGrain = _orleans.GetConstructInfoGrain(constructId);
        _constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);
        _coreUnitElementId = (await _constructElementsGrain.GetElementsOfType<CoreUnit>()).Single();
    }

    public async Task TickAsync(BehaviorContext context)
    {
        var coreUnit = await _constructElementsGrain.GetElement(_coreUnitElementId);
        var constructInfo = await _constructInfoGrain.Get();

        if (coreUnit.IsCoreDestroyed() || constructInfo.IsAbandoned())
        {
            context.NotifyConstructDestroyed(new BehaviorEventArgs(constructId, constructDefinition));
            _active = false;
            context.IsAlive = false;
            
            return;
        }
    }
}