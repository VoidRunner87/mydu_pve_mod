using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("construct")]
public class ConstructController : Controller
{
    [HttpPost]
    [Route("{constructId:long}/replace/{elementTypeName}/with/{replaceElementTypeName}")]
    public async Task<IActionResult> ReplaceElement(long constructId, string elementTypeName, string replaceElementTypeName)
    {
        var provider = ModBase.ServiceProvider;
        var elementReplacerService = provider.GetRequiredService<IElementReplacerService>();

        await elementReplacerService.ReplaceSingleElementAsync((ulong)constructId, elementTypeName, replaceElementTypeName);

        return Ok();
    }
    
    [HttpGet]
    [Route("{constructId:long}")]
    public async Task<IActionResult> Get(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var constructInfoGrain = orleans.GetConstructInfoGrain((ulong)constructId);
        var constructInfo = await constructInfoGrain.Get();

        return Ok(constructInfo);
    }

    [HttpGet]
    [Route("{constructId:long}/vel")]
    public async Task<IActionResult> GetVelocity(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var (velocity, angVelocity) = await orleans.GetConstructGrain((ulong)constructId)
            .GetConstructVelocity();

        return Ok(
            new
            {
                velocity,
                angVelocity
            }
        );
    }

    [HttpDelete]
    [Route("{constructId:long}")]
    public async Task<IActionResult> Delete(ulong constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var constructHandleRepository = provider.GetRequiredService<IConstructHandleRepository>();
        await constructHandleRepository.DeleteByConstructId(constructId);
        
        var gcGrain = orleans.GetConstructGCGrain();
        await gcGrain.DeleteConstruct(constructId);

        return Ok();
    }
    
    [HttpDelete]
    [Route("batch")]
    public async Task<IActionResult> Delete([FromBody] ulong[] constructIds)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var gcGrain = orleans.GetConstructGCGrain();

        foreach (var constructId in constructIds)
        {
            await gcGrain.DeleteConstruct(constructId);
        }

        return Ok();
    }

    [Route("{constructId:long}/shield/vent-start")]
    [HttpPost]
    public async Task<IActionResult> StartShieldVent(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var constructService = provider.GetRequiredService<IConstructService>();
        var result = await constructService.TryVentShieldsAsync((ulong)constructId);
        
        return Ok(result);
    }

    [Route("{constructId:long}/containers")]
    [HttpGet]
    public async Task<IActionResult> ListElements(ulong constructId)
    {
        var provider = ModBase.ServiceProvider;
        var bank = provider.GetGameplayBank();
        var constructElementsService = provider.GetRequiredService<IConstructElementsService>();
        var containerElements = await constructElementsService.GetContainerElements(constructId);

        var elementInfoListTask = containerElements
            .Select(x => constructElementsService.GetElement(constructId, x));

        var elementInfos = (await Task.WhenAll(elementInfoListTask))
            .Select(x => new
            {
                x.elementType,
                elementName = bank.GetDefinition(x.elementType)?.Name,
                x.position,
                x.elementId,
                x.rotation,
                serverProps = x.serverProperties.ToDictionary(
                    k => k.Key,
                    v => v.Value.value
                ),
                props = x.properties.ToDictionary(
                    k => k.Key,
                    v => v.Value.value
                )
            });
        
        return Ok(elementInfos);
    }

    [SwaggerOperation("Remove a construct's buffs, by resetting their properties back to the original values on the BO")]
    [Route("{constructId:long}/sanitize")]
    [HttpPost]
    public async Task<IActionResult> Sanitize(ulong constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();
        var bank = provider.GetGameplayBank();
        
        var constructElementsGrain = orleans.GetConstructElementsGrain(constructId);
        var elementIds = await constructElementsGrain.GetElementsOfType<Element>();

        var report = new List<string>();
        
        foreach (var elementId in elementIds)
        {
            var element = await constructElementsGrain.GetElement(elementId);
            var def = bank.GetDefinition(element.elementType);

            if (def == null)
            {
                report.Add($"Definition for {elementId} was null");
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
                        constructId = constructId,
                        elementId = elementId,
                        value = propertyValue,
                        timePoint = TimePoint.Now()
                    }
                );
                
                report.Add($"Updated {elementId} | {def.ItemType().itemType} | {def.Name} | {propName} = {propertyValue.value}");
            }
        }

        return Ok(report);
    }
}