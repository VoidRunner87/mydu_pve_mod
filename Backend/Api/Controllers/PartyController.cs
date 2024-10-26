using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Party.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("party/{playerId:long}")]
public class PartyController : Controller
{
    private readonly IServiceProvider _provider = ModBase.ServiceProvider;
    private readonly IPlayerPartyService _service = ModBase.ServiceProvider.GetRequiredService<IPlayerPartyService>();
    
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Get(ulong playerId)
    {
        var results = await _service.GetPartyByPlayerId(playerId);

        return Ok(results);
    }
}