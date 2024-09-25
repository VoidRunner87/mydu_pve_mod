using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("script")]
public class ScriptRunnerController : Controller
{
    [HttpPost]
    [Route("run/{name}")]
    public async Task<IActionResult> RunScript(string name, [FromBody] RunScriptContextRequest request)
    {
        var provider = ModBase.ServiceProvider;

        var scriptService = provider.GetRequiredService<IScriptService>();
        var result = await scriptService.ExecuteScriptAsync(
            name,
            new ScriptContext(
                provider,
                request.FactionId,
                [..request.PlayerIds],
                request.Sector,
                request.TerritoryId
            )
            {
                ConstructId = request.ConstructId,
            }
        );

        if (result.Success)
        {
            return Ok(result);
        }

        return StatusCode(500, result);
    }

    public class RunScriptContextRequest
    {
        public List<ulong> PlayerIds { get; set; } = [];
        public Vec3 Sector { get; set; }
        public ulong? ConstructId { get; set; }
        public long FactionId { get; set; } = 1;
        public Guid? TerritoryId { get; set; }
    }
}