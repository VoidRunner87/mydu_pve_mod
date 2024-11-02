using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Overrides.Actions;

public class RemoveConstructBuffsAction(IServiceProvider provider) : IModActionHandler
{
    public async Task HandleAction(ulong playerId, ModAction action)
    {
        var orleans = provider.GetRequiredService<IClusterClient>();
        var bank = provider.GetRequiredService<IGameplayBank>();

        var constructElementsGrain = orleans.GetConstructElementsGrain(action.constructId);
        var elementIds = await constructElementsGrain.GetElementsOfType<Element>();

        var logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<RemoveConstructBuffsAction>();

        foreach (var elementId in elementIds)
        {
            var element = await constructElementsGrain.GetElement(elementId);
            var def = bank.GetDefinition(element.elementType);

            if (def == null)
            {
                logger.LogInformation("Definition for {ElementId} was null", elementId);
                continue;
            }

            foreach (var dynamicProperty in def.GetDynamicProperties())
            {
                var propName = dynamicProperty.Name;

                var propertyValue = def.GetStaticPropertyOpt(propName);
                if (propertyValue == null)
                {
                    continue;
                }

                await constructElementsGrain.UpdateElementProperty(
                    new ElementPropertyUpdate
                    {
                        name = propName,
                        constructId = action.constructId,
                        elementId = elementId,
                        value = propertyValue,
                        timePoint = TimePoint.Now()
                    }
                );

                logger.LogInformation("Updated {ElementId} | {ItemType} | {DefName} | {PropName} = {Value}",
                    elementId,
                    def.ItemType().itemType,
                    def.Name,
                    propName,
                    propertyValue.value
                );
            }
        }
    }
}