using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using Mod.DynamicEncounters.Features.Party.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("party")]
public class PartyController : Controller
{
    private readonly IServiceProvider _provider = ModBase.ServiceProvider;
    private readonly IPlayerPartyService _service = ModBase.ServiceProvider.GetRequiredService<IPlayerPartyService>();

    [Route("{playerId:long}")]
    [HttpGet]
    public async Task<IActionResult> Get(ulong playerId)
    {
        var results = await _service.GetPartyByPlayerId(playerId);

        return Ok(results);
    }
    
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateParty([FromBody] PartyRequest request)
    {
        var result = await _service.CreateParty(request.InstigatorPlayerId);

        return Ok(result);
    }
    
    [HttpPost]
    [Route("disband")]
    public async Task<IActionResult> DisbandParty([FromBody] PartyRequest request)
    {
        var result = await _service.DisbandParty(request.InstigatorPlayerId);

        return Ok(result);
    }

    [HttpPost]
    [Route("invite")]
    public async Task<IActionResult> InviteToParty([FromBody] PartyRequest request)
    {
        if (request.PlayerId == 0 && !string.IsNullOrEmpty(request.PlayerName))
        {
            var playerService = _provider.GetRequiredService<IPlayerService>();
            var playerId = await playerService.FindPlayerIdByName(request.PlayerName);

            if (playerId == null)
            {
                return BadRequest($"Player '{request.PlayerName}' not found");
            }

            request.PlayerId = playerId.Value;
        }
        
        var result = await _service.InviteToParty(request.InstigatorPlayerId, request.PlayerId);

        return Ok(result);
    }

    [Route("request")]
    [HttpPost]
    public async Task<IActionResult> RequestToJoinParty([FromBody] PartyRequest request)
    {
        var result = await _service.RequestJoinParty(request.InstigatorPlayerId, request.PlayerId);

        return Ok(result);
    }

    [Route("invite/accept")]
    [HttpPost]
    public async Task<IActionResult> AcceptInvite([FromBody] PartyRequest request)
    {
        var result = await _service.AcceptPartyInvite(request.InstigatorPlayerId);

        return Ok(result);
    }
    
    [Route("invite/cancel")]
    [HttpPost]
    public async Task<IActionResult> CancelInvite([FromBody] PartyRequest request)
    {
        var result = await _service.CancelPartyInviteRequest(request.InstigatorPlayerId, request.PlayerId);

        return Ok(result);
    }

    [Route("request/accept")]
    [HttpPost]
    public async Task<IActionResult> AcceptPartyRequest([FromBody] PartyRequest request)
    {
        var result = await _service.AcceptPartyRequest(request.InstigatorPlayerId, request.PlayerId);

        return Ok(result);
    }
    
    [Route("leave")]
    [HttpPost]
    public async Task<IActionResult> LeaveParty([FromBody] PartyRequest request)
    {
        var result = await _service.LeaveParty(request.InstigatorPlayerId);

        return Ok(result);
    }
    
    [Route("promote")]
    [HttpPost]
    public async Task<IActionResult> PromoteToLeader([FromBody] PartyRequest request)
    {
        var result = await _service.PromoteToPartyLeader(request.InstigatorPlayerId, request.PlayerId);

        return Ok(result);
    }
    
    [Route("kick")]
    [HttpPost]
    public async Task<IActionResult> KickPartyMember([FromBody] PartyRequest request)
    {
        var result = await _service.KickPartyMember(request.InstigatorPlayerId, request.PlayerId);

        return Ok(result);
    }
    
    [Route("role")]
    [HttpPost]
    public async Task<IActionResult> SetPartyRole([FromBody] PartyRequest request)
    {
        var result = await _service.SetPlayerPartyRole(request.InstigatorPlayerId, request.Role);

        return Ok(result);
    }

    public class PartyRequest
    {
        public ulong InstigatorPlayerId { get; set; }
        public ulong PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string Role { get; set; }
    }
}