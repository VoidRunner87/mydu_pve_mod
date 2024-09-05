using System;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

public class ConstructDefinitionFactory(IServiceProvider provider) : IConstructDefinitionFactory
{
    public IConstructDefinition Create(PrefabItem definitionItem)
    {
        var scriptActionFactory = provider.GetRequiredService<IScriptActionFactory>();

        return new ConstructDefinition(definitionItem)
        {
            Events =
            {
                OnShieldHalfAction = scriptActionFactory.Create(definitionItem.Events.OnShieldHalf),
                OnShieldLowAction = scriptActionFactory.Create(definitionItem.Events.OnShieldLow),
                OnShieldDownAction = scriptActionFactory.Create(definitionItem.Events.OnShieldDown),
                OnCoreStressHigh = scriptActionFactory.Create(definitionItem.Events.OnCoreStressHigh),
                OnDestruction = scriptActionFactory.Create(definitionItem.Events.OnDestruction),
            }
        };
    }
}