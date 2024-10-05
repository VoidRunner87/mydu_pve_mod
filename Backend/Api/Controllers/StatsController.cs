using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Features.Sector.Services;
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
        return Ok(new
        {
            BehaviorStats = StatsRecorder.GetStats(),
            ConstructsPendingDelete = ConstructsPendingDelete.Data.Count
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