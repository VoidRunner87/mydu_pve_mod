using System;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using BotLib.Generated;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQ.Visibility;
using NQutils.Def;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("construct")]
public class ConstructController : Controller
{
    [HttpPost]
    [Route("{constructId:long}/replace/{elementTypeName}/with/{replaceElementTypeName}")]
    public async Task<IActionResult> ReplaceElement(long constructId, string elementTypeName, string replaceElementTypeName)
    {
        var provider = ModBase.ServiceProvider;
        var elementReplacerService = provider.GetRequiredService<IElementReplacerService>();

        await elementReplacerService.ReplaceSingleElementAsync((ulong)constructId, elementTypeName, replaceElementTypeName);

        return Ok();
    }
    
    [HttpPost]
    [Route("enginepower/{constructId:long}/elementid/{elementId}")]
    public async Task<IActionResult> SetEnginePower(long constructId, long elementId)
    {
        await Task.Yield();
        var provider = ModBase.ServiceProvider;

        var elementPropertyUpdate = new ElementPropertyUpdate
        {
            constructId = (ulong)constructId,
            name = "engine_power",
            elementId = (ulong)elementId,
            value = new PropertyValue(1d),
            timePoint = TimePoint.Now()
        };

        var update = NQutils.Serialization.Grpc.MakePacket(
            new NQutils.Messages.ElementPropertyUpdate(elementPropertyUpdate)
        );

        var internalClient = provider.GetRequiredService<Internal.InternalClient>();
        await internalClient.PublishConstructEventAsync(
            new ConstructEvent
            {
                ConstructId = (ulong)constructId,
                Message = update,
                ElementLOD = (uint)ElementLOD.LOD_NONE,
                RadarVisible = true
            }
        );

        return Ok();
    }

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
    [Route("{constructId:long}/forward")]
    public async Task<IActionResult> AddPosition(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var constructInfoGrain = orleans.GetConstructInfoGrain((ulong)constructId);
        var constructInfo = await constructInfoGrain.Get();

        var forward = VectorMathUtils.GetForward(constructInfo.rData.rotation.ToQuat());

        const float accel = 100 * 9.81f;
        var offset = accel * forward * 1000;
        
        provider.CreateLogger<ConstructController>()
            .LogInformation("{V}", offset);
        
        await ModBase.Bot.Req.ConstructUpdate(
            new ConstructUpdate
            {
                constructId = (ulong)constructId,
                rotation = constructInfo.rData.rotation,
                position = constructInfo.rData.position + offset.ToNqVec3(),
                worldAbsoluteVelocity = offset.ToNqVec3(),
                worldRelativeVelocity = offset.ToNqVec3(),
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

    [HttpDelete]
    [Route("{constructId:long}")]
    public async Task<IActionResult> Delete(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var gcGrain = orleans.GetConstructGCGrain();
        await gcGrain.DeleteConstruct((ulong)constructId);

        return Ok();
    }

    [Route("{constructId:long}/shield/vent-start")]
    [HttpPost]
    public async Task<IActionResult> StartShieldVent(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var constructService = provider.GetRequiredService<IConstructService>();
        var result = await constructService.TryVentShieldsAsync((ulong)constructId);
        
        return Ok(result);
    }

    [Route("{constructId:long}/weapons")]
    [HttpGet]
    public async Task<IActionResult> GetWeapons(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var constructElementsService = provider.GetRequiredService<IConstructElementsService>();
        var bank = provider.GetGameplayBank();

        var weaponsElements = await constructElementsService.GetWeaponUnits((ulong)constructId);
        var elementInfos = await Task.WhenAll(
            weaponsElements.Select(id => constructElementsService.GetElement((ulong)constructId, id))
        );
        
        var weaponUnits = elementInfos
            .Select(ei => new AggressiveBehavior.WeaponHandle(ei, bank.GetBaseObject<WeaponUnit>(ei)!))
            .Where(w => w.Unit is not StasisWeaponUnit)
            .ToList()
            .Select(x => new
            {
                x.ElementInfo.elementId
            });

        return Ok(weaponUnits);
    }

    [Route("{constructId:long}/warp-start")]
    [HttpPost]
    public async Task<IActionResult> WarpStart(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();
        var constructInfoGrain = orleans.GetConstructInfoGrain((ulong)constructId);
        var constructInfo = await constructInfoGrain.Get();

        var pilot = constructInfo.mutableData.pilot;

        if (pilot == null)
        {
            return BadRequest("Not Piloted");
        }

        var constructGrain = orleans.GetConstructGrain((ulong)constructId);
        await constructGrain.WarpStart(pilot.Value);
        
        return Ok();
    }
    
    [Route("{constructId:long}/warp-end")]
    [HttpPost]
    public async Task<IActionResult> WarpStop(long constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();
        var constructInfoGrain = orleans.GetConstructInfoGrain((ulong)constructId);
        var constructInfo = await constructInfoGrain.Get();

        var pilot = constructInfo.mutableData.pilot;

        if (pilot == null)
        {
            return BadRequest("Not Piloted");
        }

        var constructGrain = orleans.GetConstructGrain((ulong)constructId);
        await constructGrain.WarpEnd(pilot.Value);

        return Ok();
    }

    [Route("{constructId:long}/apply-stasis")]
    [HttpPost]
    public async Task<IActionResult> ApplyStasisToConstruct(long constructId, [FromBody] StasisRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var constructInfoGrain = orleans.GetConstructInfoGrain((ulong)constructId);
        await constructInfoGrain.Update(new ConstructInfoUpdate
        {
            additionalMaxSpeedDebuf = new MaxSpeedDebuf
            {
                until = (DateTime.Now + request.DurationSpan).ToNQTimePoint(),
                value = Math.Clamp(request.Value, -2d, 2d)
            }
        });

        return Ok();
    }

    public class StasisRequest
    {
        public TimeSpan DurationSpan { get; set; }
        public double Value { get; set; }
    }
}