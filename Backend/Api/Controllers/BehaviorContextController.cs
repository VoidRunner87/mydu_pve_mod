using Microsoft.AspNetCore.Mvc;
using NQ;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("behavior/context")]
public class BehaviorContextController : Controller
{
    [SwaggerOperation("Reads the behavior context data of an NPC construct")]
    [HttpGet]
    [Route("{constructId:long}")]
    public IActionResult Get(ulong constructId)
    {
        if (!ConstructBehaviorContextCache.Data.TryGetValue(constructId, out var context))
        {
            return NotFound();
        }
        
        return Ok(context);
    }

    [SwaggerOperation("Sets the target position of an NPC construct. You can control it like a strategy game. But I meant it for debugging tho (for now)")]
    [HttpPost]
    [Route("{constructId:long}/set/target-move-position")]
    public IActionResult SetTargetMovePosition(ulong constructId, SetTargetPositionRequest request)
    {
        if (!ConstructBehaviorContextCache.Data.TryGetValue(constructId, out var context))
        {
            return NotFound();
        }

        context.DisableAutoTargetMovePosition();
        context.TargetMovePosition = request.Position;

        return Ok();
    }

    public class SetTargetPositionRequest
    {
        public Vec3 Position { get; set; }
    }
}