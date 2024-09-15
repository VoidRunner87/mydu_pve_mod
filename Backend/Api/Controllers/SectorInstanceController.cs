using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Sector.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("sector/instance")]
public class SectorInstanceController(IServiceProvider provider) : Controller
{
    private readonly ISectorInstanceRepository _repository = provider.GetRequiredService<ISectorInstanceRepository>();
    
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _repository.GetAllAsync());
    }

    [HttpPost]
    [Route("expire/all")]
    public async Task<IActionResult> ExpireAll()
    {
        await _repository.ExpireAllAsync();

        return Ok();
    }
    
    [HttpPost]
    [Route("expire/force/all")]
    public async Task<IActionResult> ForceExpireAll()
    {
        await _repository.ForceExpireAllAsync();

        return Ok();
    }
}