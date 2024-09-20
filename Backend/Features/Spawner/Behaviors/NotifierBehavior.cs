using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.Logging;
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

public class NotifierBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private List<ElementId> _weaponsElements;
    private IClusterClient _orleans;
    private IGameplayBank _bank;
    private IConstructGrain _constructGrain;
    private ILogger<AggressiveBehavior> _logger;
    private IConstructElementsGrain _constructElementsGrain;

    private ElementId _coreUnitElementId;
    
    private bool _active = true;

    public bool IsActive() => _active;

    public async Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _orleans = provider.GetOrleans();

        _constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);

        _bank = provider.GetGameplayBank();

        _weaponsElements = await _constructElementsGrain.GetElementsOfType<WeaponUnit>();
        var elementInfos = await Task.WhenAll(
            _weaponsElements.Select(_constructElementsGrain.GetElement)
        );

        _coreUnitElementId = (await _constructElementsGrain.GetElementsOfType<CoreUnit>()).SingleOrDefault();

        _constructGrain = _orleans.GetConstructGrain(constructId);
        
        context.ExtraProperties.TryAdd("CORE_ID", _coreUnitElementId);
        
        context.IsAlive = _coreUnitElementId.elementId > 0;
        _active = context.IsAlive;
        
        _logger = provider.CreateLogger<AggressiveBehavior>();
    }

    public async Task TickAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;

        var coreUnit = await _constructElementsGrain.GetElement(_coreUnitElementId);

        if (coreUnit.IsCoreStressHigh())
        {
            await context.NotifyCoreStressHighAsync(new BehaviorEventArgs(constructId, prefab, context));
        }

        var constructInfoGrain = _orleans.GetConstructInfoGrain(constructId);
        var constructInfo = await constructInfoGrain.Get();

        if (constructInfo.IsShieldLowerThanHalf())
        {
            await context.NotifyShieldHpHalfAsync(new BehaviorEventArgs(constructId, prefab, context));
        }
        
        if (constructInfo.IsShieldLowerThan25())
        {
            await context.NotifyShieldHpLowAsync(new BehaviorEventArgs(constructId, prefab, context));
        }
        
        if (constructInfo.IsShieldDown())
        {
            // var constructElementGrain = _orleans.GetConstructElementsGrain(constructId);
            // var shieldElementId = (await constructElementGrain.GetElementsOfType<ShieldGeneratorUnit>()).First();
            //
            // await constructElementGrain.RepairElement(shieldElementId, 1);
            //
            // foreach (var we in _weaponsElements)
            // {
            //     await constructElementGrain.RepairElement(we, 1);
            // }

            // _constructGrain = _orleans.GetConstructGrain(constructId);
            // await _constructGrain.UpdateConstructInfo(new ConstructInfoUpdate
            // {
            //     constructId = constructId,
            //     shieldState = new ShieldState
            //     {
            //         hasShield = true,
            //         isVenting = true
            //     },
            // });
            // var constructFightGrain = _orleans.GetConstructFightGrain(constructId);
            // await constructFightGrain.StartVenting(ModBase.Bot.PlayerId);
            
            await context.NotifyShieldHpDownAsync(new BehaviorEventArgs(constructId, prefab, context));
        }
    }
}