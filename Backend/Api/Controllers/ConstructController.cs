using System.Threading.Tasks;
using BotLib.Generated;
using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("construct")]
public class ConstructController : Controller
{
    [HttpGet]
    [Route("{constructId:long}")]
    public async Task<IActionResult> Get(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var constructInfoGrain = orleans.GetConstructInfoGrain((ulong)constructId);
        var constructInfo = await constructInfoGrain.Get();
        
        return Ok(constructInfo);
    }

    [HttpPost]
    [Route("pos/add/{constructId:long}")]
    public async Task<IActionResult> AddPosition(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var constructInfoGrain = orleans.GetConstructInfoGrain((ulong)constructId);
        var constructInfo = await constructInfoGrain.Get();

        await ModBase.Bot.Req.ConstructUpdate(
            new ConstructUpdate
            {
                constructId = (ulong)constructId,
                rotation = constructInfo.rData.rotation,
                position = constructInfo.rData.position + new Vec3{x = 1000},
                pilotId = 10000,
                time = TimePoint.Now()
            }
        );

        return Ok();
    }

    [HttpPost]
    [Route("{fromConstructId:long}/lookat/{toConstructId:long}")]
    public async Task<IActionResult> LookAt(long fromConstructId, long toConstructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();
        
        var fromConstructInfoGrain = orleans.GetConstructInfoGrain((ulong)fromConstructId);
        var fromConstructInfo = await fromConstructInfoGrain.Get();
        var fromPos = fromConstructInfo.rData.position;
        
        var toConstructInfoGrain = orleans.GetConstructInfoGrain((ulong)toConstructId);
        var toConstructInfo = await toConstructInfoGrain.Get();
        var toPos = toConstructInfo.rData.position;

        var desiredRotation = VectorMathUtils.SetRotationToMatchDirection(
            fromPos.ToVector3(), 
            toPos.ToVector3()
        );

        await ModBase.Bot.Req.ConstructUpdate(
            new ConstructUpdate
            {
                constructId = (ulong)fromConstructId,
                position = fromPos,
                time = TimePoint.Now(),
                grounded = false,
                rotation = desiredRotation.ToNqQuat()
            }
        );
        
        return Ok();
    }

    [HttpGet]
    [Route("vel/{constructId:long}")]
    public async Task<IActionResult> GetVelocity(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var (velocity, angVelocity) = await orleans.GetConstructGrain((ulong)constructId)
            .GetConstructVelocity();

        return Ok(
            new
            {
                velocity,
                angVelocity
            }
        );
    }
}