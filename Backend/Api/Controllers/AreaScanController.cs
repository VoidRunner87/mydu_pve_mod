using System;
using System.Threading.Tasks;
using Backend.Scenegraph;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("area-scan")]
public class AreaScanController : Controller
{
    [Route("players")]
    [HttpPost]
    public async Task<IActionResult> PlayerScan([FromBody] AreaScanRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var areaScanService = provider.GetRequiredService<IAreaScanService>();

        var pos = await request.GetReferencePosition(provider);
        
        var contacts =
            await areaScanService.ScanForPlayerContacts(request.ConstructId ?? 1, pos, request.Radius, request.Limit);

        return Ok(contacts);
    }
    
    [Route("asteroids")]
    [HttpPost]
    public async Task<IActionResult> AsteroidScan([FromBody] AreaScanRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var areaScanService = provider.GetRequiredService<IAreaScanService>();

        var pos = await request.GetReferencePosition(provider);
        
        var contacts =
            await areaScanService.ScanForPlayerContacts(request.ConstructId ?? 1, pos, request.Radius, request.Limit);

        return Ok(contacts);
    }
    
    [Route("npc")]
    [HttpPost]
    public async Task<IActionResult> NpcScan([FromBody] 
        
        AreaScanRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var areaScanService = provider.GetRequiredService<IAreaScanService>();

        var pos = await request.GetReferencePosition(provider);
        
        var contacts =
            await areaScanService.ScanForNpcConstructs(pos, request.Radius, request.Limit);

        return Ok(contacts);
    }

    public class AreaScanRequest
    {
        [JsonProperty] public ulong? ConstructId { get; set; }
        [JsonProperty] public Vec3? Position { get; set; }
        [JsonProperty] public double Radius { get; set; } = DistanceHelpers.OneSuInMeters * 20;
        [JsonProperty] public int Limit { get; set; } = 20;

        public async Task<Vec3> GetReferencePosition(IServiceProvider provider)
        {
            if (Position.HasValue) return Position.Value;
            if (!ConstructId.HasValue) return new Vec3();
            
            var sceneGraph = provider.GetRequiredService<IScenegraph>();
            return await sceneGraph.GetConstructCenterWorldPosition(ConstructId.Value);
        }
    }
}