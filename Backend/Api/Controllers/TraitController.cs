using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("trait")]
public class TraitController(IServiceProvider provider) : Controller
{
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var traitRepository = provider.GetRequiredService<ITraitRepository>();

        return Ok((await traitRepository.Get()).Map());
    }
}