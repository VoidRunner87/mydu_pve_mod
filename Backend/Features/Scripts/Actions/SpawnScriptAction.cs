using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.AWS;
using Backend.Fixture;
using Backend.Fixture.Construct;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
using NQ;
using NQ.Interfaces;
using NQutils.Config;
using NQutils.Sql;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class SpawnScriptAction(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "spawn";

    private IPointGenerator _pointGenerator;
    private ILogger<SpawnScriptAction> _logger;
    public string Name => ActionName;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;

        _logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<SpawnScriptAction>();

        _pointGenerator = provider
            .GetRequiredService<IPointGeneratorFactory>()
            .Create(actionItem.Area);

        var random = provider
            .GetRequiredService<IRandomProvider>()
            .GetRandom();

        var spawnCount = random.Next(actionItem.MinQuantity, actionItem.MaxQuantity);

        _logger.LogInformation("Generated {SpawnCount} Spawn Items", spawnCount);

        var tasks = Enumerable.Repeat(
            () => SpawnOneAsync(context),
            spawnCount
        );

        foreach (var t in tasks)
        {
            // TODO Has to be in sequence because of file read issues with the S3 class
            await t();
        }

        return ScriptActionResult.Successful();
    }

    private async Task<ScriptActionResult> SpawnOneAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var orleans = provider.GetOrleans();
        var constructHandleRepo = provider.GetRequiredService<IConstructHandleRepository>();

        var random = provider.GetRequiredService<IRandomProvider>()
            .GetRandom();

        var constructDefinitionRepo = provider.GetRequiredService<IPrefabItemRepository>();
        var constructDefItem = await constructDefinitionRepo.FindAsync(actionItem.Prefab);

        if (constructDefItem == null)
        {
            return ScriptActionResult.Failed();
        }

        var constructDefinitionFactory = provider.GetRequiredService<IConstructDefinitionFactory>();
        var constructDef = constructDefinitionFactory.Create(constructDefItem);

        var s3 = provider.GetRequiredService<IS3>();

        var settings = Config.Instance.wrecks;
        settings.override_path = constructDef.DefinitionItem.Folder;

        var constructJson = await s3.Get(
            settings,
            constructDef.DefinitionItem.Path
        );

        using var source = FixtureSource.FromStringContent(
            constructJson,
            FixtureKind.Construct
        );

        var spawnPoint = _pointGenerator.NextPoint(random);

        var actionPosition = actionItem.Position ?? new Vec3();
        var spawnPosition = context.Sector + spawnPoint + actionPosition;

        var prefabConstructName = constructDef.DefinitionItem.ServerProperties.Header.PrettyName;
        var overrideName = actionItem.Override.ConstructName;

        var resultName = string.IsNullOrEmpty(overrideName) ? prefabConstructName : overrideName;

        if (string.IsNullOrEmpty(resultName))
        {
            resultName = $"E-{random.Next(1000, 9999)}";
        }

        var fixture = ConstructFixture.FromSource(source);
        fixture.parentId = actionItem.Override.PositionParentId;
        fixture.header.prettyName = resultName;
        fixture.ownerId = new EntityId { playerId = constructDef.DefinitionItem.OwnerId };
        fixture.position = spawnPosition;
        fixture.isUntargetable = constructDef.DefinitionItem.IsUntargetable;
        fixture.isNPC = constructDef.DefinitionItem.IsNpc;
        fixture.serverProperties.dynamicFixture = true;
        fixture.serverProperties.isDynamicWreck = constructDef.DefinitionItem.ServerProperties.IsDynamicWreck;
        fixture.header.constructIdHint = null;

        var constructId = (await ConstructFixtureImport.Import(
            fixture,
            provider.GetRequiredService<IUserContent>(),
            provider.GetRequiredService<ISql>(),
            provider.GetRequiredService<IVoxelImporter>(),
            provider.GetRequiredService<IGameplayBank>(),
            provider.GetRequiredService<IRDMSStorage>(),
            provider.GetRequiredService<IPlanetList>()
        ))[0];

        context.ConstructId = constructId;
        
        _logger.LogInformation("Spawned Construct [{Name}]({Id}) at ::pos{{0,0,{Pos}}}", resultName, constructId,
            spawnPosition);
        
        var isWreck = constructDef.DefinitionItem.ServerProperties.IsDynamicWreck;
        
        var behaviorList = new List<string>();

        if (!isWreck)
        {
            behaviorList.AddRange(["alive", "select-target", "notifier"]);
            behaviorList.AddRange(constructDefItem.InitialBehaviors);
        }

        // Keeping track of what this script instance spawned
        await constructHandleRepo.AddAsync(
            new ConstructHandleItem
            {
                ConstructId = constructId,
                ConstructDefinitionId = constructDef.Id,
                Id = Guid.NewGuid(),
                Sector = context.Sector,
                ConstructDefinitionItem = constructDef.DefinitionItem,
                OriginalOwnerPlayerId = constructDef.DefinitionItem.OwnerId,
                OriginalOrganizationId = 0,
                FactionId = context.FactionId ?? 1,
                JsonProperties = new ConstructHandleProperties
                {
                    Tags = actionItem.Tags,
                    Behaviors = behaviorList,
                },
                OnCleanupScript = constructDef.DefinitionItem.ServerProperties.IsDynamicWreck
                    ? "despawn-wreck"
                    : "despawn"
            }
        );

        _logger.LogInformation("Created Handle for {ConstructId} on {Sector}", constructId, context.Sector);

        try
        {
            if (!isWreck)
            {
                await context.ServiceProvider
                    .GetRequiredService<IConstructService>()
                    .ActivateShieldsAsync(constructId);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Failed to Set Shields to Active. Will try again another time with construct Behaviors");
        }

        if (isWreck)
        {
            // TODO obtain time span from territory
            // TODO add territory to sector instance
            await provider.GetRequiredService<IConstructService>()
                .SetAutoDeleteFromNowAsync(constructId, TimeSpan.FromHours(3));
        }

        var actionFactory = provider.GetRequiredService<IScriptActionFactory>();
        var onLoadAction = actionFactory.Create(actionItem.Events.OnLoad);

        try
        {
            // Execute on load tasks
            await onLoadAction.ExecuteAsync(
                context.WithConstructId(constructId)
            );
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Failed to Execute OnLoad Action for Script {Script}",
                JsonConvert.SerializeObject(onLoadAction)
            );
        }
        
        try
        {
            await orleans.GetConstructParentingGrain().ReloadConstruct(constructId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Reload Construct Post-Spawn");
        }

        return ScriptActionResult.Successful();
    }

    public string GetKey() => Name;
}