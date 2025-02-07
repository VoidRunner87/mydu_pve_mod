using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotLib.BotClient;
using BotLib.Protocols;
using BotLib.Protocols.Queuing;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Helpers;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Serilog.Events;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("maintenance")]
public class MaintenanceController : Controller
{
    [HttpDelete]
    [Route("bugged-wrecks")]
    public async Task<IActionResult> CleanupBuggedWrecks()
    {
        var provider = ModBase.ServiceProvider;
        var handleRepository = provider.GetRequiredService<IConstructHandleRepository>();
        var constructService = provider.GetRequiredService<IConstructService>();

        var items = await handleRepository.FindAllBuggedPoiConstructsAsync();

        foreach (var constructId in items)
        {
            await constructService.SoftDeleteAsync(constructId);
        }
        
        return Ok(items);
    }

    [HttpPost]
    [Route("grpc/reconnect")]
    public async Task<IActionResult> ReconnectGrpc()
    {
        var provider = ModBase.ServiceProvider;
        var clientFactory = provider.GetRequiredService<IDuClientFactory>();
        ClientExtensions.UseFactory(clientFactory);

        var pi = LoginInformations.BotLogin(
            Environment.GetEnvironmentVariable("BOT_PREFIX")!,
            Environment.GetEnvironmentVariable("BOT_LOGIN")!,
            Environment.GetEnvironmentVariable("BOT_PASSWORD")!
        );

        await clientFactory.Connect(pi, allowExisting: true);
        
        return Ok();
    }

    [Route("loglevel/{logLevel:int}")]
    [HttpPost]
    public IActionResult ChangeLogLevel(int logLevel)
    {
        LoggingConfiguration.LoggingLevelSwitch.MinimumLevel = (LogEventLevel)logLevel;

        return Ok();
    }

    [Route("constructs/rebuff")]
    [HttpPost]
    public async Task<IActionResult> RebuffConstructs()
    {
        var provider = ModBase.ServiceProvider;
        var factory = provider.GetRequiredService<IPostgresConnectionFactory>();

        using var db = factory.Create();
        db.Open();

        var results = (await db.QueryAsync(
            """
            SELECT id FROM public.construct
            WHERE json_properties->>'kind' = '4' AND
                  deleted_at IS NULL
            """
        )).ToList();

        var constructIds = results.Select(x => (ulong)x.id);

        var taskQueueService = provider.GetRequiredService<ITaskQueueService>();

        var counter = 0;
        
        foreach (var constructId in constructIds)
        {
            await taskQueueService.EnqueueScript(
                new ScriptActionItem
                {
                    Type = "remove-buffs",
                    ConstructId = constructId
                },
                DateTime.UtcNow
            );
            
            await taskQueueService.EnqueueScript(
                new ScriptActionItem
                {
                    Type = "buff",
                    ConstructId = constructId
                },
                DateTime.UtcNow
            );

            counter++;
        }

        return Ok($"Enqueued {counter} Operations");
    }

    [HttpPost]
    public async Task<IActionResult> GetConstructsOnArea([FromBody] ClearConstructsRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var areaScanService = provider.GetRequiredService<IAreaScanService>();

        var constructService = provider.GetRequiredService<IConstructService>();
        var constructTransform = await constructService.GetConstructTransformAsync(request.ConstructId);

        if (!constructTransform.ConstructExists)
        {
            return NotFound();
        }

        var npcConstructs = await areaScanService.ScanForNpcConstructs(constructTransform.Position, request.Radius, request.Limit);
        var abandonedConstructs = await areaScanService.ScanForAbandonedConstructs(constructTransform.Position, request.Radius, request.Limit);

        var list = new List<ScanContact>();
        list.AddRange(npcConstructs);
        list.AddRange(abandonedConstructs);
        
        return Ok(list.OrderBy(x => x.Distance));
    }

    public class ClearConstructsRequest
    {
        public bool SkipAbandoned { get; set; }
        public bool SkipNpcs { get; set; }
        public double Radius { get; set; } = 20 * DistanceHelpers.OneSuInMeters;
        public int Limit { get; set; } = 10;
        public ulong ConstructId { get; set; }
    }
}