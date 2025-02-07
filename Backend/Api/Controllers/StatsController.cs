using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Features.Sector.Services;
using Mod.DynamicEncounters.Threads.Handles;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("stats")]
public class StatsController : Controller
{
    [SwaggerOperation("Retrieves Stats")]
    [HttpGet]
    [Route("")]
    public ActionResult Get()
    {
        var process = Process.GetCurrentProcess();
        
        return Ok(new
        {
            ThreadCount = process.Threads.Count,
            BehaviorStats = StatsRecorder.GetStats(),
            CustomStats = StatsRecorder.GetCustomStats(),
            ConstructsPendingDelete = ConstructsPendingDelete.Data.Count,
            ConstructHandles = ConstructBehaviorLoop.ConstructHandles.Select(x => x.Key),
            LoopHeartBeatSpan = LoopStats.LastHeartbeatMap
                .ToDictionary(
                    k => k.Key,
                    v => DateTime.UtcNow - v.Value
                )
                .OrderBy(x => x.Key)
        });
    }
    
    [SwaggerOperation("Clears Stats")]
    [HttpDelete]
    [Route("")]
    public ActionResult Delete()
    {
        StatsRecorder.ClearAll();
        
        return Ok();
    }
}