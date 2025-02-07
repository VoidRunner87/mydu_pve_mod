using System;
using Backend;
using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Helpers;
using NQutils.Def;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("config")]
public class ConfigController : Controller
{
    private readonly IServiceProvider _provider = ModBase.ServiceProvider;
    
    [SwaggerOperation("Retrieves Garbage Collection Settings")]
    [Route("gc")]
    [HttpGet]
    public IActionResult GetConstructGcConfig()
    {
        var bank = _provider.GetGameplayBank();

        var gc = bank.GetBaseObject<ConstructGCConfig>();
        
        return Ok(new
        {
            gc.abandonedConstructDeleteDelayHours,
        });
    }
}