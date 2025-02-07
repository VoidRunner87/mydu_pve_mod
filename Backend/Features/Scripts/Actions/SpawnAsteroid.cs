using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend;
using Backend.Scenegraph;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
using NQ;
using NQ.Interfaces;
using NQutils.Def;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class SpawnAsteroid(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "spawn-asteroid";
    public string Name => ActionName;
    public string GetKey() => Name;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var random = context.ServiceProvider.GetRequiredService<IRandomProvider>().GetRandom();
        var pointGeneratorFactory = context.ServiceProvider.GetRequiredService<IPointGeneratorFactory>();
        var orleans = context.ServiceProvider.GetOrleans();
        var asteroidManagerGrain = orleans.GetAsteroidManagerGrain();
        var constructService = context.ServiceProvider.GetRequiredService<IConstructService>();
        var bank = context.ServiceProvider.GetGameplayBank();
        var sceneGraph = context.ServiceProvider.GetRequiredService<IScenegraph>();

        var number = random.Next(1, 100);
        var properties = actionItem.GetProperties<Properties>();

        var minTier = properties.MinTier;
        var maxTier = properties.MaxTier + 1;
        var isPublished = properties.Published;
        var center = properties.Center ?? context.Sector;

        var tier = random.Next(minTier, maxTier);

        var pointGenerator = pointGeneratorFactory.Create(actionItem.Area);
        var position = center + pointGenerator.NextPoint(random);

        var asteroidId = await asteroidManagerGrain.SpawnAsteroid(
            properties.TierOverride ?? tier,
            $"{properties.FileNamePrefix}_{tier}_{number}.json",
            position,
            properties.PlanetId
        );

        var info = await constructService.GetConstructInfoAsync(asteroidId);

        if (info.Info == null)
        {
            return ScriptActionResult.Failed();
        }

        var name = info.Info.rData.name
            .Replace("A-", "R-");

        await constructService.RenameConstruct(asteroidId, name);

        var asteroidCenterPos = await sceneGraph.GetConstructCenterWorldPosition(asteroidId);

        var asteroidManagerConfig = bank.GetBaseObject<AsteroidManagerConfig>();
        var deletePoiTimeSpan = properties.AutoDeleteTimeSpan ??
                                TimeSpan.FromDays(asteroidManagerConfig.lifetimeDays);

        if (isPublished)
        {
            await asteroidManagerGrain.ForcePublish(asteroidId);

            var spawnScriptAction = new SpawnScriptAction(
                new ScriptActionItem
                {
                    Area = new ScriptActionAreaItem { Type = "null" },
                    Prefab = properties.PointOfInterestPrefabName,
                    Override = new ScriptActionOverrides
                    {
                        ConstructName = name,
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "AddConstructHandle", false }
                    }
                });

            var spawnContext = new ScriptContext(
                context.ServiceProvider,
                context.FactionId,
                context.PlayerIds,
                asteroidCenterPos,
                context.TerritoryId)
            {
                Properties = context.Properties
            };

            var spawnResult = await spawnScriptAction.ExecuteAsync(spawnContext);

            if (!spawnResult.Success)
            {
                return spawnResult;
            }

            if (!spawnContext.ConstructId.HasValue)
            {
                return spawnResult;
            }

            await constructService.SetAutoDeleteFromNowAsync(
                spawnContext.ConstructId.Value,
                deletePoiTimeSpan
            );

            await context.ServiceProvider.GetRequiredService<ITaskQueueService>()
                .EnqueueScript(new ScriptActionItem
                    {
                        Type = "delete",
                        ConstructId = spawnContext.ConstructId.Value
                    },
                    DateTime.UtcNow + deletePoiTimeSpan);
        }

        if (properties.HiddenFromDsat)
        {
            await context.ServiceProvider.GetRequiredService<IAsteroidService>()
                .HideFromDsatListAsync(asteroidId);

            await context.ServiceProvider.GetRequiredService<ITaskQueueService>()
                .EnqueueScript(new ScriptActionItem
                    {
                        Type = "delete-asteroid",
                        ConstructId = asteroidId
                    },
                    DateTime.UtcNow + deletePoiTimeSpan);
        }

        var scriptActionFactory = context.ServiceProvider.GetRequiredService<IScriptActionFactory>();
        var action = scriptActionFactory.Create(actionItem.Actions);

        var actionContext = new ScriptContext(
            context.ServiceProvider,
            context.FactionId,
            context.PlayerIds,
            asteroidCenterPos,
            context.TerritoryId)
        {
            Properties = context.Properties
        };

        var actionResult = await action.ExecuteAsync(actionContext);

        await Task.Delay(500);

        return actionResult;
    }

    public class Properties
    {
        [JsonProperty] public int MinTier { get; set; }
        [JsonProperty] public int MaxTier { get; set; }
        [JsonProperty] public bool Published { get; set; }
        [JsonProperty] public Vec3? Center { get; set; }
        [JsonProperty] public string PointOfInterestPrefabName { get; set; } = "poi-asteroid";
        [JsonProperty] public string FileNamePrefix { get; set; } = "basic";
        [JsonProperty] public ulong PlanetId { get; set; } = 2;
        [JsonProperty] public TimeSpan? AutoDeleteTimeSpan { get; set; }
        [JsonProperty] public int? TierOverride { get; set; }

        /// <summary>
        /// Does not show on DSAT but deletes automatically.
        /// </summary>
        [JsonProperty]
        public bool HiddenFromDsat { get; set; }
    }
}