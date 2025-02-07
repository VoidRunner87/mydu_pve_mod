using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Scenegraph;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Extensions;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.Spawner.Interfaces;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NQ;
using NQ.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("asteroid")]
public class AsteroidController : Controller
{
    public class SpawnRequest
    {
        public double Distance { get; set; } = 100;
        public ulong? ConstructId { get; set; }
        public string File { get; set; }
        public Vec3? Position { get; set; }
        public JToken Data { get; set; }
    }

    [HttpPost]
    [Route("waypoints")]
    public async Task<IActionResult> GetAsteroidWaypoints([FromBody] AsteroidWaypointRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var repository = provider.GetRequiredService<IConstructRepository>();
        var travelRouteService = provider.GetRequiredService<ITravelRouteService>();

        var asteroids = await repository.FindAsteroids();
        var route = travelRouteService.Solve(
            new WaypointItem
            {
                Position = request.FromPosition,
            },
            asteroids.Select(x => new WaypointItem
            {
                Position = x.Position,
                Name = x.Name
            }));
        
        return Ok(route);
    }

    [HttpDelete]
    [Route("")]
    public async Task<IActionResult> DeleteAsteroidAround([FromBody] DeleteAsteroidsAroundRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var areaScanService = provider.GetRequiredService<IAreaScanService>();
        var sceneGraph = provider.GetRequiredService<IScenegraph>();
        var taskQueueService = provider.GetRequiredService<ITaskQueueService>();

        var pos = await sceneGraph.GetConstructCenterWorldPosition(request.ConstructId);

        var result = await areaScanService.ScanForAsteroids(pos, request.Radius);

        foreach (var contact in result)
        {
            await taskQueueService.EnqueueScript(new ScriptActionItem
            {
                Type = "delete-asteroid",
                ConstructId = contact.ConstructId
            }, DateTime.UtcNow);
        }

        return Ok();
    }

    [HttpPost]
    [Route("spawn-v2")]
    public async Task<IActionResult> SpawnV2Async([FromBody] SpawnRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var sceneGraph = provider.GetRequiredService<IScenegraph>();
        var random = provider.GetRequiredService<IRandomProvider>().GetRandom();

        if (!request.Position.HasValue && request.ConstructId.HasValue)
        {
            var constructPos = await sceneGraph.GetConstructCenterWorldPosition(request.ConstructId.Value);

            var offset = random.RandomDirectionVec3() * request.Distance;
            var pos = offset + constructPos;

            request.Position = pos;
        }
        else if (!request.Position.HasValue)
        {
            return BadRequest();
        }

        var asteroidSpawnerService = provider.GetRequiredService<IAsteroidSpawnerService>();
        var outcome = await asteroidSpawnerService.SpawnAsteroidWithData(new SpawnAsteroidCommand
        {
            Position = request.Position.Value,
            Planet = 2,
            Prefix = "T",
            Tier = 5,
            RegisterAsteroid = true,
            Data = request.Data,
            Radius = 32,
            AreaSize = 256,
            VoxelSize = 512,
            VoxelLod = 7,
            Gravity = 0
        });
        
        return Ok(outcome);
    }

    [HttpPost]
    [Route("test")]
    public IActionResult Test([FromBody] TestRequest req)
    {
        var bufferSize = Lz4CompressionService.ReadDecompressedSize(req.Data);
        var decompResult = Lz4CompressionService.Decompress(req.Data.SkipBytes(4), bufferSize);
        var stringResult = Encoding.UTF8.GetString(decompResult);
        var compResult = Lz4CompressionService.Compress(stringResult);
        var decomp2Result = Lz4CompressionService.Decompress(compResult, bufferSize);
        var stringResult2 = Encoding.UTF8.GetString(decomp2Result);
        
        return Ok(
            stringResult == stringResult2
        );
    }

    public class TestRequest
    {
        public byte[] Data { get; set; } = [];
    }

    [SwaggerOperation("Spawns an Asteroid at a position or around a construct")]
    [HttpPost]
    [Route("spawn")]
    public async Task<IActionResult> SpawnAsync([FromBody] SpawnRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();
        var sceneGraph = provider.GetRequiredService<IScenegraph>();

        if (!request.Position.HasValue && request.ConstructId.HasValue)
        {
            var constructPos = await sceneGraph.GetConstructCenterWorldPosition(request.ConstructId.Value);

            var random = provider.GetRequiredService<IRandomProvider>().GetRandom();

            var offset = random.RandomDirectionVec3() * 200;
            var pos = offset + constructPos;

            request.Position = pos;
        }
        else if (!request.Position.HasValue)
        {
            return BadRequest();
        }

        var asteroidManagerGrain = orleans.GetAsteroidManagerGrain();
        var asteroidId = await asteroidManagerGrain.SpawnAsteroid(
            1, request.File, request.Position.Value, 2
        );

        await asteroidManagerGrain.ForcePublish(asteroidId);

        return Ok(asteroidId);
    }

    public class DeleteAsteroidsAroundRequest
    {
        [JsonProperty] public ulong ConstructId { get; set; }
        [JsonProperty] public double Radius { get; set; } = DistanceHelpers.OneSuInMeters * 5;
    }

    public class AsteroidWaypointRequest
    {
        [JsonProperty] public Vec3 FromPosition { get; set; }
    }
}