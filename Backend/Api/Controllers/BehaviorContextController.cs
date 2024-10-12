using System.Linq;
using System.Threading.Tasks;
using Backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Vector;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using NQ;
using NQutils.Def;
using NQutils.Sql;
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

        return Ok(new
        {
            Context = context,
            ExtraProperties = context.Properties.ToDictionary(k => k.Key, v => v.Value)
        });
    }

    [SwaggerOperation("Sets the target position of an NPC construct and sticks with that position until changed")]
    [HttpPost]
    [Route("target-move-position")]
    public async Task<IActionResult> SetTargetMovePosition(ulong constructId, [FromBody] SetTargetPositionRequest request)
    {
        if (!request.Position.HasValue && !request.TargetConstructId.HasValue && !request.FromPlayerIdWaypoint.HasValue)
        {
            return BadRequest(
                $"Need {nameof(request.Position)} or {nameof(request.TargetConstructId)} or {nameof(request.FromPlayerIdWaypoint)}");
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

        if (request.TargetConstructId.HasValue)
        {
            context.DisableAutoTargetMovePosition();

            var constructService = ModBase.ServiceProvider.GetRequiredService<IConstructService>();
            var transformOutcome = await constructService.GetConstructTransformAsync(request.TargetConstructId.Value);

            if (!transformOutcome.ConstructExists)
            {
                return NotFound($"Target Construct {request.TargetConstructId} Not Found");
            }

            context.SetTargetMovePosition(transformOutcome.Position);
        }

        if (request.FromPlayerIdWaypoint.HasValue)
        {
            var sql = ModBase.ServiceProvider
                .GetRequiredService<ISql>();

            var playerWaypoint = await sql.ReadPlayerProperty(
                request.FromPlayerIdWaypoint.Value,
                Character.d_currentWaypoint
            );

            if (!string.IsNullOrEmpty(playerWaypoint))
            {
                context.DisableAutoTargetMovePosition();
                
                var position = playerWaypoint.PositionToVec3();
                context.SetTargetMovePosition(position);
            }
        }

        return Ok(
            new
            {
                TargetConstructId = context.GetTargetConstructId(),
                TargetMovePosition = context.GetTargetMovePosition()
            }
        );
    }

    [SwaggerOperation("Sets the target construct to attack")]
    [Route("target-construct")]
    [HttpPost]
    public async Task<IActionResult> SetTargetConstruct(ulong constructId, [FromBody] SetTargetConstructRequest request)
    {
        if (!ConstructBehaviorContextCache.Data.TryGetValue(constructId, out var context))
        {
            return NotFound($"NPC Construct {constructId} Not Found");
        }

        if (request.TargetConstructId.HasValue)
        {
            var constructService = ModBase.ServiceProvider.GetRequiredService<IConstructService>();
            var exists = await constructService.Exists(request.TargetConstructId.Value);

            if (!exists)
            {
                return NotFound($"Target Construct {request.TargetConstructId} Not Found");
            }

            context.DisableAutoSelectAttackTargetConstruct();
            context.SetTargetConstructId(request.TargetConstructId.Value);
        }

        return Ok(
            new
            {
                TargetConstructId = context.GetTargetConstructId(),
                TargetMovePosition = context.GetTargetMovePosition()
            }
        );
    }

    [SwaggerOperation("Releases the NPC to pick their target position automatically and move automatically")]
    [HttpDelete]
    [Route("reset")]
    public IActionResult ResetMoveAndTarget(ulong constructId)
    {
        if (!ConstructBehaviorContextCache.Data.TryGetValue(constructId, out var context))
        {
            return NotFound();
        }

        context.EnableAutoSelectAttackTargetConstruct();
        context.EnableAutoTargetMovePosition();
        
        return Ok();
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
        public ulong? TargetConstructId { get; set; }
        public ulong? FromPlayerIdWaypoint { get; set; }
    }

    public class SetTargetConstructRequest
    {
        public ulong? TargetConstructId { get; set; }
    }
}