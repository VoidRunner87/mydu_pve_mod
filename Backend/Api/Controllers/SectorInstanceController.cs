using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using NQ;

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
    [Route("activate")]
    public async Task<IActionResult> ActivateSector([FromBody] SectorRequest request)
    {
        var sectorInstance = await _repository.FindBySector(request.Sector);

        if (sectorInstance == null)
        {
            return NotFound();
        }
        
        var sectorPoolManager = provider.GetRequiredService<ISectorPoolManager>();

        await sectorPoolManager.ActivateSector(sectorInstance);

        return Ok(sectorInstance);
    }
    
    [HttpPost]
    [Route("expire")]
    public async Task<IActionResult> ExpireSector([FromBody] SectorRequest request)
    {
        var sectorInstance = await _repository.FindBySector(request.Sector);

        if (sectorInstance == null)
        {
            return NotFound();
        }
        
        var sectorPoolManager = provider.GetRequiredService<ISectorPoolManager>();

        await sectorPoolManager.SetExpirationFromNow(request.Sector, TimeSpan.Zero);

        return Ok();
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

    [HttpGet]
    [Route("grid")]
    public IActionResult GetGrid()
    {
        return Ok(SectorGridConstructCache.Data);
    }
    
    public class SectorRequest
    {
        public Vec3 Sector { get; set; }
    }
}