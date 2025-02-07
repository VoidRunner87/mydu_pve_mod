using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("pid")]
public class PIDConfigurationController : Controller
{
    [HttpPost]
    [Route("")]
    public IActionResult Update([FromBody] PidConfigUpdateRequest request)
    {
        PIDMovementEffect.Kp = request.Kp;
        PIDMovementEffect.Ki = request.Ki;
        PIDMovementEffect.Kd = request.Kd;
        
        return Ok();
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            PIDMovementEffect.Kp,
            PIDMovementEffect.Ki,
            PIDMovementEffect.Kd,
        });
    }

    public class PidConfigUpdateRequest
    {
        public double Kp { get; set; }
        public double Ki { get; set; }
        public double Kd { get; set; }
    }
}