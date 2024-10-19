using System;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Database.Services;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Repository;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Events;
using Mod.DynamicEncounters.Features.ExtendedProperties;
using Mod.DynamicEncounters.Features.Faction;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Loot;
using Mod.DynamicEncounters.Features.NQ;
using Mod.DynamicEncounters.Features.Quests;
using Mod.DynamicEncounters.Features.Scripts.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Services;
using Mod.DynamicEncounters.Features.Sector;
using Mod.DynamicEncounters.Features.Services;
using Mod.DynamicEncounters.Features.Spawner;
using Mod.DynamicEncounters.Features.TaskQueue;

namespace Mod.DynamicEncounters.Features;

public static class FeaturesRegistration
{
    public static void RegisterModFeatures(this IServiceCollection services)
    {
        services.AddSingleton<IPostgresConnectionFactory, PostgresConnectionFactory>();
        services.AddSingleton<IRandomProvider, DefaultRandomProvider>();
        services.AddSingleton<IFeatureReaderService, FeatureService>();
        services.AddSingleton<IFeatureWriterService, FeatureService>();
        services.AddSingleton<IScriptLoaderService, FileSystemScriptLoaderService>();
        services.AddSingleton<IConstructSpatialHashRepository, ConstructSpatialHashRepository>();
        services.AddSingleton<ISectorSpatialHashCacheService, SectorSpatialHashCacheServiceService>();
        services.AddSingleton<IBlueprintSpawnerService, BlueprintSpawnerService>();
        services.AddSingleton<IConstructService>(p =>
            new CachedConstructService(
                new ConstructService(p),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(10)
            )
        );
        services.AddSingleton<IConstructElementsService>(p =>
            new CachedConstructElementsService(
                new ConstructElementsService(p),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5)
            )
        );
        services.AddSingleton<INpcRadarService>(p => new CachedAreaScanService(new AreaScanService(p)));
        services.AddSingleton<IErrorRepository, ErrorRepository>();
        services.AddSingleton<IErrorService, ErrorService>();
        services.AddSingleton<IErrorService, ErrorService>();
        services.AddSingleton<IPlayerAlertService, PlayerAlertService>();

        services.RegisterSectorGeneration();
        services.RegisterSpawnerScripts();
        services.RegisterTaskQueue();
        services.RegisterEvents();
        services.RegisterNqServices();
        services.RegisterLootSystem();
        services.RegisterFaction();
        services.RegisterExtendedProperties();
        services.RegisterQuests();
    }
}