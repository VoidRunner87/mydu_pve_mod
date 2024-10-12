using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Helpers.DU;
using Mod.DynamicEncounters.Threads.Handles;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class AliveCheckBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private ElementId _coreUnitElementId;

    private IConstructHandleRepository _handleRepository;
    private IConstructService _constructService;
    private IConstructElementsService _constructElementsService;
    private ILogger<AliveCheckBehavior> _logger;

    public BehaviorTaskCategory Category => BehaviorTaskCategory.HighPriority;

    public async Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;

        _handleRepository = provider.GetRequiredService<IConstructHandleRepository>();
        _constructService = provider.GetRequiredService<IConstructService>();
        _constructElementsService = provider.GetRequiredService<IConstructElementsService>();
        _coreUnitElementId = await _constructElementsService.GetCoreUnit(constructId);
        _logger = provider.CreateLogger<AliveCheckBehavior>();
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive || !context.IsBehaviorActive<AliveCheckBehavior>())
        {
            await context.NotifyConstructDestroyedAsync(new BehaviorEventArgs(constructId, prefab, context));
            await _handleRepository.RemoveHandleAsync(constructId);
            ConstructBehaviorLoop.ConstructHandles.TryRemove(constructId, out _);

            _logger.LogInformation("Construct {Construct} NOT ALIVE", constructId);
            
            return;
        }

        // just to cache it
        await Task.WhenAll([
            _constructElementsService.GetAllSpaceEnginesPower(constructId),
            _constructElementsService.GetFunctionalDamageWeaponCount(constructId)
        ]).OnError(
            exception =>
            {
                _logger.LogError(exception, "Failed to query Weapon and Engines");
                foreach (var e in exception.InnerExceptions)
                {
                    _logger.LogError(e, "Inner Exception");
                }
            }
        );

        var coreUnit = await _constructElementsService.NoCache().GetElement(constructId, _coreUnitElementId);
        var constructInfoOutcome = await _constructService.GetConstructInfoAsync(constructId);
        var constructInfo = constructInfoOutcome.Info;

        if (!constructInfoOutcome.ConstructExists)
        {
            context.IsAlive = false;
            context.Deactivate<AliveCheckBehavior>();
            return;
        }
        
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
            
            _logger.LogInformation("Construct {Construct} CORE DESTROYED", constructId);
            
            return;
        }
        
        ConstructBehaviorContextCache.Data.ResetExpiration(constructId);

        await _constructService.ActivateShieldsAsync(constructId);
        
        ConstructBehaviorLoop.RecordConstructHeartBeat(constructId);
    }
}