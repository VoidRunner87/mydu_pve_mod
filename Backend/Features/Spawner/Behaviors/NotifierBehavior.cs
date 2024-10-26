using System;
using System.Collections.Generic;
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
using Mod.DynamicEncounters.Helpers.DU;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class NotifierBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private IClusterClient _orleans;
    private ILogger<NotifierBehavior> _logger;
    private IConstructElementsGrain _constructElementsGrain;

    private ElementId _coreUnitElementId;
    
    private bool _active = true;
    private IConstructService _constructService;
    private IConstructElementsService _constructElementsService;

    public bool IsActive() => _active;

    public BehaviorTaskCategory Category => BehaviorTaskCategory.MediumPriority;

    public async Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _orleans = provider.GetOrleans();

        _constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);
        _constructElementsService = provider.GetRequiredService<IConstructElementsService>();

        _coreUnitElementId = (await _constructElementsGrain.GetElementsOfType<CoreUnit>()).SingleOrDefault();

        _constructService = provider.GetRequiredService<IConstructService>();
        
        context.Properties.TryAdd("CORE_ID", _coreUnitElementId);
        
        context.IsAlive = _coreUnitElementId.elementId > 0;
        _active = context.IsAlive;
        
        _logger = provider.CreateLogger<NotifierBehavior>();
    }

    public async Task TickAsync(BehaviorContext context)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            { "ConstructId", constructId }
        });
        
        // TODO consider a better place for this in the future
        context.ClearExpiredTimerProperties();

        try
        {
            var enginePower = Math.Clamp(await _constructElementsService.GetAllSpaceEnginesPower(constructId), 0, 1);
            _logger.LogInformation("Construct {Construct} Engine Power: {Power}", constructId, enginePower);

            context.SetProperty(BehaviorContext.EnginePowerProperty, enginePower);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Construct {Construct} Failed to fetch engine power", constructId);
        }

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

        // TODO move this to a separate behavior - VentBehavior
        context.TryGetProperty("ShieldVentTimer", out var shieldVentTimer, 0d);
        shieldVentTimer += context.DeltaTime;

        if (constructInfo.IsShieldDown())
        {
            if (shieldVentTimer > 5)
            {
                await _constructService.TryVentShieldsAsync(constructId);
                shieldVentTimer = 0;
            }

            await context.NotifyShieldHpDownAsync(new BehaviorEventArgs(constructId, prefab, context));
        }
        
        context.SetProperty("ShieldVentTimer", shieldVentTimer);
    }
}