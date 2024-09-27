using Microsoft.AspNetCore.Mvc;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("stats")]
public class StatsController : Controller
{
    [HttpGet]
    [Route("")]
    public ActionResult Get()
    {
        var (moveMinTime, moveMaxTime, moveOccurrences) = StatsRecorder.GetMovementStats();
        var (targetMinTime, targetMaxTime, targetOccurrences) = StatsRecorder.GetTargetingStats();

        return Ok(new
        {
            moveMinTime,
            moveMaxTime,
            moveOccurrences,
            targetMinTime,
            targetMaxTime,
            targetOccurrences
        });
    }
    
    [HttpDelete]
    [Route("")]
    public ActionResult Delete()
    {
        StatsRecorder.Clear();
        
        return Ok();
    }
}