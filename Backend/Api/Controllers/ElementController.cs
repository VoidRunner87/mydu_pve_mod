
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("element")]
public class ElementController : Controller
{
    [HttpGet]
    [Route("databank/construct/{constructId:long}/element/{elementId:long}")]
    public async Task<IActionResult> GetDataBankData(ulong constructId, ulong elementId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var constructElementsGrain = orleans.GetConstructElementsGrain(constructId);
        var propertyValue = await constructElementsGrain.GetPropertyValue(new ElementPropertyId
        {
            elementId = elementId,
            constructId = constructId,
            name = "databank"
        });

        return Ok(propertyValue.value);
    }

}