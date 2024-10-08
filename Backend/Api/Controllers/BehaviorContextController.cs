using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using NQ;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("behavior/context/{constructId:long}")]
public class BehaviorContextController : Controller
{
    [SwaggerOperation("Reads the behavior context data of an NPC construct")]
    [HttpGet]
    [Route("")]
    public IActionResult Get(ulong constructId)
    {
        if (!ConstructBehaviorContextCache.Data.TryGetValue(constructId, out var context))
        {
            return NotFound();
        }

        return Ok(context);
    }

    [SwaggerOperation("Sets the target position of an NPC construct and sticks with that position until changed")]
    [HttpPost]
    [Route("target-move-position")]
    public async Task<IActionResult> SetTargetMovePosition(ulong constructId, SetTargetPositionRequest request)
    {
        if (!request.Position.HasValue && !request.ConstructId.HasValue)
        {
            return BadRequest($"Need {nameof(request.Position)} or {nameof(request.ConstructId)}");
        }

        if (!ConstructBehaviorContextCache.Data.TryGetValue(constructId, out var context))
        {
            return NotFound($"NPC Construct {constructId} Not Found");
        }

        if (request.Position.HasValue)
        {
            context.DisableAutoTargetMovePosition();
            context.SetTargetMovePosition(request.Position.Value);
        }

        if (request.ConstructId.HasValue)
        {
            context.DisableAutoTargetMovePosition();

            var constructService = ModBase.ServiceProvider.GetRequiredService<IConstructService>();
            var constructInfo = await constructService.GetConstructInfoAsync(request.ConstructId.Value);

            if (constructInfo == null)
            {
                return NotFound($"Target Construct {request.ConstructId} Not Found");
            }

            context.SetTargetMovePosition(constructInfo.rData.position);
        }

        return Ok(
            new
            {
                context.TargetConstructId,
                context.TargetMovePosition
            }
        );
    }

    [SwaggerOperation("Sets the target construct to attack")]
    [Route("target-construct")]
    [HttpPost]
    public async Task<IActionResult> SetTargetConstruct(ulong constructId, SetTargetConstructRequest request)
    {
        if (!ConstructBehaviorContextCache.Data.TryGetValue(constructId, out var context))
        {
            return NotFound($"NPC Construct {constructId} Not Found");
        }

        if (request.ConstructId.HasValue)
        {
            var constructService = ModBase.ServiceProvider.GetRequiredService<IConstructService>();
            var constructInfo = await constructService.GetConstructInfoAsync(request.ConstructId.Value);

            if (constructInfo == null)
            {
                return NotFound($"Target Construct {request.ConstructId} Not Found");
            }

            context.DisableAutoSelectAttackTargetConstruct();
            context.SetTargetConstructId(request.ConstructId.Value);
        }

        return Ok(
            new
            {
                context.TargetConstructId,
                context.TargetMovePosition
            }
        );
    }

    [SwaggerOperation("Releases the NPC to pick their target position automatically")]
    [HttpDelete]
    [Route("target-move-position")]
    public IActionResult RemoveManualMovePosition(ulong constructId)
    {
        if (!ConstructBehaviorContextCache.Data.TryGetValue(constructId, out var context))
        {
            return NotFound();
        }

        context.EnableAutoTargetMovePosition();

        return Ok();
    }

    [SwaggerOperation("Releases the NPC to pick targets to attack automatically")]
    [HttpDelete]
    [Route("target-construct")]
    public IActionResult RemoveManualTargetConstruct(ulong constructId)
    {
        if (!ConstructBehaviorContextCache.Data.TryGetValue(constructId, out var context))
        {
            return NotFound();
        }

        context.EnableAutoSelectAttackTargetConstruct();

        return Ok();
    }

    public class SetTargetPositionRequest
    {
        public Vec3? Position { get; set; }
        public ulong? ConstructId { get; set; }
    }

    public class SetTargetConstructRequest
    {
        public ulong? ConstructId { get; set; }
    }
}