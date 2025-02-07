using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Warp.Data;
using Mod.DynamicEncounters.Features.Warp.Interfaces;
using Mod.DynamicEncounters.Vector.Helpers;
using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("warp")]
public class WarpController : Controller
{
    [HttpPost]
    [Route("anchor/v2")]
    public async Task<IActionResult> CreateWarpAnchorV2([FromBody] WarpAnchorRequestV2 request)
    {
        if (request.PlayerId == default)
        {
            return BadRequest();
        }

        var provider = ModBase.ServiceProvider;
        var warpAnchorService = provider.GetRequiredService<IWarpAnchorService>();

        var outcome = await warpAnchorService.SpawnWarpAnchor(
            new SpawnWarpAnchorCommand
            {
                FromPosition = request.FromPosition,
                TargetPosition = request.TargetPosition,
                ElementTypeName = request.ElementTypeName,
                PlayerId = request.PlayerId
            }
        );

        if (!outcome.Success)
        {
            return BadRequest();
        }

        return Ok(
            new WarpAnchorResponse(
                outcome.WarpAnchorConstructId.constructId,
                outcome.WarpAnchorConstructName,
                outcome.WarpAnchorPosition,
                outcome.WarpAnchorPosition.Vec3ToPosition()
            )
        );
    }

    [HttpPost]
    [Route("cooldown")]
    public async Task<IActionResult> SetCooldown([FromBody] SetWarpPropertyRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var warpAnchorService = provider.GetRequiredService<IWarpAnchorService>();
        var outcome = await warpAnchorService.SetWarpCooldown(new SetWarpCooldownCommand
        {
            ConstructId = request.ConstructId,
            ElementTypeName = request.ElementTypeName
        });

        return Ok(outcome);
    }

    public class SetWarpPropertyRequest
    {
        [JsonProperty] public ulong ConstructId { get; set; }
        [JsonProperty] public string ElementTypeName { get; set; } = string.Empty;
    }

    public class WarpAnchorRequestV2
    {
        public ulong PlayerId { get; set; }
        public Vec3 FromPosition { get; set; }
        public Vec3 TargetPosition { get; set; }
        public string ElementTypeName { get; set; } = "WarpDrive";
    }

    public class WarpAnchorResponse(ulong constructId, string constructName, Vec3 position, string positionString)
    {
        public ulong ConstructId { get; set; } = constructId;
        public string ConstructName { get; set; } = constructName;
        public Vec3 Position { get; set; } = position;
        public string PositionString { get; set; } = positionString;
    }
}