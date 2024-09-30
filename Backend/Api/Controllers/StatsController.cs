using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("stats")]
public class StatsController : Controller
{
    [SwaggerOperation("Retrieves Behavior Thread Stats")]
    [HttpGet]
    [Route("")]
    public ActionResult Get()
    {
        return Ok(StatsRecorder.GetStats());
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