using Microsoft.AspNetCore.Mvc;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("stats")]
public class StatsController : Controller
{
    [HttpGet]
    [Route("")]
    public ActionResult Get()
    {
        return Ok(StatsRecorder.GetStats());
    }
    
    [HttpDelete]
    [Route("")]
    public ActionResult Delete()
    {
        StatsRecorder.ClearAll();
        
        return Ok();
    }
}