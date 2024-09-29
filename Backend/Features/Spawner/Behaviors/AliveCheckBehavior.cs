using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers.DU;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class AliveCheckBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private ElementId _coreUnitElementId;

    private IConstructHandleRepository _handleRepository;
    private IConstructService _constructService;
    private IConstructElementsService _constructElementsService;

    public BehaviorTaskCategory Category => BehaviorTaskCategory.HighPriority;

    public async Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;

        _handleRepository = provider.GetRequiredService<IConstructHandleRepository>();
        _constructService = provider.GetRequiredService<IConstructService>();
        _constructElementsService = provider.GetRequiredService<IConstructElementsService>();
        _coreUnitElementId = await _constructElementsService.GetCoreUnit(constructId);
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

        // just to cache it
        await Task.WhenAll([
            _constructElementsService.GetAllSpaceEnginesPower(constructId),
            _constructElementsService.GetFunctionalDamageWeaponCount(constructId)
        ]);

        var coreUnit = await _constructElementsService.NoCache().GetElement(constructId, _coreUnitElementId);
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
            
            return;
        }

        await _constructService.ActivateShieldsAsync(constructId);
    }
}