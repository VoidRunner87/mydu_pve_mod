using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("safezone")]
public class SafeZoneController : Controller
{
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Get()
    {
        var provider = ModBase.ServiceProvider;
        var safeZoneService = provider.GetRequiredService<ISafeZoneService>();

        return Ok(await safeZoneService.GetSafeZones());
    }

    [HttpPost]
    [Route("check-inside-safe-zone")]
    public async Task<IActionResult> CheckInsideSafeZone([FromBody] CheckInsideSafeZoneRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var safeZoneService = provider.GetRequiredService<ISafeZoneService>();

        var safeZones = await safeZoneService.GetSafeZones();

        return Ok(safeZones.Select(sz => new
        {
            sz,
            inside = sz.IsPointInside(request.Position)
        }));
    }

    public class CheckInsideSafeZoneRequest
    {
        public Vec3 Position { get; set; }
    }
}