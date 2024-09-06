using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Sector.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("sector/encounter")]
public class SectorEncountersController(IServiceProvider provider) : Controller
{
    private readonly ISectorEncounterRepository _repository =
        provider.GetRequiredService<ISectorEncounterRepository>();
    
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAll()
    {
        return Json(await _repository.GetAllAsync());
    }
}