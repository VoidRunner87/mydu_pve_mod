using System;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.AWS;
using Backend.Database;
using Backend.Fixture;
using Backend.Fixture.Construct;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Config;
using NQutils.Def;
using NQutils.Sql;
using Orleans;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class SpawnScriptAction(ScriptActionItem actionItem) : IScriptAction
{
    private IPointGenerator _pointGenerator;
    private ILogger<SpawnScriptAction> _logger;
    public string Name { get; } = Guid.NewGuid().ToString();

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

        var constructDefinitionRepo = provider.GetRequiredService<IConstructDefinitionItemRepository>();
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
        fixture.parentId = null;
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

        _logger.LogInformation("Spawned Construct({Id}) at ::pos{{0,0,{Pos}}}", constructId, spawnPosition);

        var clusterClient = provider.GetRequiredService<IClusterClient>();
        await clusterClient.GetConstructParentingGrain().ReloadConstruct(constructId);

        var constructElementsGrain = orleans.GetConstructElementsGrain(constructId);
        var shields = await constructElementsGrain.GetElementsOfType<ShieldGeneratorUnit>();

        if (!constructDef.DefinitionItem.ServerProperties.IsDynamicWreck && shields.Count > 0)
        {
            var sql = provider.GetRequiredService<ISql>();
            await sql.SetShieldEnabled(constructId, true);

            var constructGrain = orleans.GetConstructGrain(constructId);
            await constructGrain.UpdateConstructInfo(new ConstructInfoUpdate
            {
                shieldState = new ShieldState
                {
                    hasShield = true,
                    isActive = true,
                    shieldHpRatio = 1
                }
            });
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
                JsonProperties = new ConstructHandleProperties
                {
                    Tags = actionItem.Tags
                },
                OnCleanupScript = constructDef.DefinitionItem.ServerProperties.IsDynamicWreck
                    ? "despawn-wreck"
                    : "despawn"
            }
        );

        _logger.LogInformation("Created Handle for {ConstructId} on {Sector}", constructId, context.Sector);

        return ScriptActionResult.Successful();
    }

    public string GetKey() => Name;
}