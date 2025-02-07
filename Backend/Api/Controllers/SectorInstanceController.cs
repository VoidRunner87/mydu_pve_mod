using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Sector.Data;
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

    [HttpGet]
    [Route("active")]
    public async Task<IActionResult> GetActiveSector()
    {
        return Ok(await _repository.FindActiveAsync());
    }

    [HttpPost]
    [Route("activate")]
    public async Task<IActionResult> ActivateSector([FromBody] SectorRequest request)
    {
        SectorInstance sectorInstance;

        if (request.Sector.HasValue)
        {
            sectorInstance = await _repository.FindBySector(request.Sector.Value);
        }
        else if (request.Id.HasValue)
        {
            sectorInstance = await _repository.FindById(request.Id.Value);
        }
        else
        {
            return BadRequest();
        }

        if (sectorInstance == null)
        {
            return NotFound();
        }
        
        var sectorPoolManager = provider.GetRequiredService<ISectorPoolManager>();

        await sectorPoolManager.ForceActivateSector(sectorInstance.Id);

        return Ok(sectorInstance);
    }
    
    [HttpPost]
    [Route("expire")]
    public async Task<IActionResult> ExpireSector([FromBody] SectorRequest request)
    {
        SectorInstance sectorInstance;

        if (request.Sector.HasValue)
        {
            sectorInstance = await _repository.FindBySector(request.Sector.Value);
        }
        else if (request.Id.HasValue)
        {
            sectorInstance = await _repository.FindById(request.Id.Value);
        }
        else
        {
            return BadRequest();
        }
        
        if (sectorInstance == null)
        {
            return NotFound();
        }
        
        var sectorPoolManager = provider.GetRequiredService<ISectorPoolManager>();

        await sectorPoolManager.SetExpirationFromNow(sectorInstance.Sector, TimeSpan.Zero);

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
    
    public class SectorRequest
    {
        public Vec3? Sector { get; set; }
        public Guid? Id { get; set; }
    }
}