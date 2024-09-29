using System;
using System.Threading.Tasks;
using Backend;
using Backend.AWS;
using Backend.Fixture;
using Backend.Fixture.Construct;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using NQutils.Sql;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class BlueprintSpawnerService(IServiceProvider provider) : IBlueprintSpawnerService
{
    public async Task<ulong> SpawnAsync(SpawnArgs args)
    {
        var s3 = provider.GetRequiredService<IS3>();

        var settings = NQutils.Config.Config.Instance.wrecks;
        settings.override_path = args.Folder;

        var constructJson = await s3.Get(
            settings,
            args.File
        );

        using var source = FixtureSource.FromStringContent(
            constructJson,
            FixtureKind.Construct
        );

        var fixture = ConstructFixture.FromSource(source);
        fixture.parentId = null;
        fixture.header.prettyName = args.Name;
        fixture.ownerId = args.OwnerEntityId;
        fixture.position = args.Position;
        fixture.isUntargetable = args.IsUntargetable;
        fixture.isNPC = args.IsNpc;
        fixture.serverProperties.dynamicFixture = true;
        fixture.serverProperties.isDynamicWreck = args.IsDynamicWreck;
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

        return constructId;
    }
}