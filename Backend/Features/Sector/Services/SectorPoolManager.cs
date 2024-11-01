﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Common.Vector;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Events.Data;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Sector.Services;

public class SectorPoolManager(IServiceProvider serviceProvider) : ISectorPoolManager
{
    public const double SectorGridSnap = DistanceHelpers.OneSuInMeters * 20;

    private readonly IRandomProvider _randomProvider = serviceProvider.GetRequiredService<IRandomProvider>();

    private readonly ISectorInstanceRepository _sectorInstanceRepository =
        serviceProvider.GetRequiredService<ISectorInstanceRepository>();

    private readonly IConstructHandleManager _constructHandleManager =
        serviceProvider.GetRequiredService<IConstructHandleManager>();

    private readonly IConstructSpatialHashRepository _constructSpatial =
        serviceProvider.GetRequiredService<IConstructSpatialHashRepository>();

    private readonly ILogger<SectorPoolManager> _logger = serviceProvider.CreateLogger<SectorPoolManager>();

    public async Task GenerateSectors(SectorGenerationArgs args)
    {
        var count = await _sectorInstanceRepository.GetCountWithTagAsync(args.Tag);
        var missingQuantity = args.Quantity - count;

        if (missingQuantity <= 0)
        {
            _logger.LogDebug("No Sectors Missing. Missing {Missing} of {Total}", missingQuantity, args.Quantity);
            return;
        }

        var handleCount = await _constructHandleManager.GetActiveCount();
        var featureReaderService = serviceProvider.GetRequiredService<IFeatureReaderService>();
        var maxBudgetConstructs = await featureReaderService.GetIntValueAsync("MaxConstructHandles", 50);

        if (handleCount >= maxBudgetConstructs)
        {
            _logger.LogError("Generate Sector: Reached MAX Number of Construct Handles to Spawn: {Max}",
                maxBudgetConstructs);
            return;
        }

        var allSectorInstances = await _sectorInstanceRepository.GetAllAsync();
        var sectorInstanceMap = allSectorInstances
            .Select(k => k.Sector.GridSnap(SectorGridSnap * args.SectorMinimumGap).ToLongVector3())
            .ToHashSet();

        var random = _randomProvider.GetRandom();

        var randomMinutes = random.Next(0, 60);

        for (var i = 0; i < missingQuantity; i++)
        {
            if (!args.Encounters.Any())
            {
                continue;
            }

            var encounter = random.PickOneAtRandom(args.Encounters);

            // TODO
            var radius = MathFunctions.Lerp(
                encounter.Properties.MinRadius,
                encounter.Properties.MaxRadius,
                random.NextDouble()
            );

            Vec3 position;
            var interactions = 0;
            const int maxInteractions = 100;

            do
            {
                position = random.RandomDirectionVec3() * radius;
                position += encounter.Properties.CenterPosition;
                position = position.GridSnap(SectorGridSnap);

                interactions++;
            } while (
                interactions < maxInteractions ||
                sectorInstanceMap
                    .Contains(
                        position.GridSnap(SectorGridSnap * args.SectorMinimumGap).ToLongVector3()
                    )
            );

            var instance = new SectorInstance
            {
                Id = Guid.NewGuid(),
                Sector = position,
                FactionId = args.FactionId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow + encounter.Properties.ExpirationTimeSpan +
                            TimeSpan.FromMinutes(randomMinutes * i),
                TerritoryId = encounter.TerritoryId,
                OnLoadScript = encounter.OnLoadScript,
                OnSectorEnterScript = encounter.OnSectorEnterScript,
            };

            if (position is { x: 0, y: 0, z: 0 })
            {
                _logger.LogWarning("BLOCKED Sector 0,0,0 creation");
                return;
            }

            try
            {
                await _sectorInstanceRepository.AddAsync(instance);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Failed to create sector. Likely violating unique constraint. It will be tried again on the next cycle");
            }

            await Task.Delay(200);
        }
    }

    public async Task GenerateTerritorySectors(SectorGenerationArgs args)
    {
        var count = await _sectorInstanceRepository.GetCountByTerritoryAsync(args.TerritoryId);
        var missingQuantity = args.Quantity - count;

        if (missingQuantity <= 0)
        {
            // _logger.LogInformation("No Territory({Territory}) Sectors Missing. Missing {Missing} of {Total}", args.TerritoryId, missingQuantity, args.Quantity);
            return;
        }

        var handleCount = await _constructHandleManager.GetActiveCount();
        var featureReaderService = serviceProvider.GetRequiredService<IFeatureReaderService>();
        var maxBudgetConstructs = await featureReaderService.GetIntValueAsync("MaxConstructHandles", 50);

        if (handleCount >= maxBudgetConstructs)
        {
            _logger.LogError(
                "Generate Territory({Territory}) Sector: Reached MAX Number of Construct Handles to Spawn: {Max}",
                args.TerritoryId,
                maxBudgetConstructs
            );
            return;
        }

        var allSectorInstances = await _sectorInstanceRepository.GetAllAsync();
        var sectorInstanceMap = allSectorInstances
            .Select(k => k.Sector.GridSnap(SectorGridSnap * args.SectorMinimumGap).ToLongVector3())
            .ToHashSet();

        var random = _randomProvider.GetRandom();

        var randomMinutes = random.Next(0, 60);

        for (var i = 0; i < missingQuantity; i++)
        {
            if (!args.Encounters.Any())
            {
                continue;
            }

            var encounter = random.PickOneAtRandom(args.Encounters);

            // TODO
            var radius = MathFunctions.Lerp(
                encounter.Properties.MinRadius,
                encounter.Properties.MaxRadius,
                random.NextDouble()
            );

            Vec3 position;
            var interactions = 0;
            const int maxInteractions = 100;

            do
            {
                position = random.RandomDirectionVec3() * radius;
                position += encounter.Properties.CenterPosition;
                position = position.GridSnap(SectorGridSnap);

                interactions++;
            } while (
                interactions < maxInteractions ||
                sectorInstanceMap
                    .Contains(
                        position.GridSnap(SectorGridSnap * args.SectorMinimumGap).ToLongVector3()
                    )
            );

            var instance = new SectorInstance
            {
                Id = Guid.NewGuid(),
                Sector = position,
                FactionId = args.FactionId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow + encounter.Properties.ExpirationTimeSpan +
                            TimeSpan.FromMinutes(randomMinutes * i),
                TerritoryId = args.TerritoryId,
                OnLoadScript = encounter.OnLoadScript,
                OnSectorEnterScript = encounter.OnSectorEnterScript,
            };

            if (position is { x: 0, y: 0, z: 0 })
            {
                _logger.LogWarning("BLOCKED Sector 0,0,0 creation");
                return;
            }

            try
            {
                await _sectorInstanceRepository.AddAsync(instance);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Failed to create sector. Likely violating unique constraint. It will be tried again on the next cycle");
            }

            await Task.Delay(200);
        }
    }

    public async Task LoadUnloadedSectors()
    {
        var scriptService = serviceProvider.GetRequiredService<IScriptService>();
        var unloadedSectors = (await _sectorInstanceRepository.FindUnloadedAsync()).ToList();

        if (unloadedSectors.Count == 0)
        {
            _logger.LogInformation("No Sectors {Count} Need Loading", unloadedSectors.Count);
            return;
        }

        var handleCount = await _constructHandleManager.GetActiveCount();
        var featureReaderService = serviceProvider.GetRequiredService<IFeatureReaderService>();
        var maxBudgetConstructs = await featureReaderService.GetIntValueAsync("MaxConstructHandles", 50);

        if (handleCount >= maxBudgetConstructs)
        {
            _logger.LogError("LoadUnloadedSectors: Reached MAX Number of Construct Handles to Spawn: {Max}",
                maxBudgetConstructs);
            return;
        }

        foreach (var sector in unloadedSectors)
        {
            try
            {
                await scriptService.ExecuteScriptAsync(
                    sector.OnLoadScript,
                    new ScriptContext(
                        serviceProvider,
                        sector.FactionId,
                        [],
                        sector.Sector,
                        sector.TerritoryId
                    )
                    {
                        // TODO properties for OnLoadScript
                        // Properties = 
                    }
                ).OnError(exception =>
                {
                    _logger.LogError(exception, "Failed to Execute On Load Script (Aggregate)");

                    foreach (var e in exception.InnerExceptions)
                    {
                        _logger.LogError(e, "Failed to Execute On Load Script");
                    }
                });

                await Task.Delay(200);
                await _sectorInstanceRepository.SetLoadedAsync(sector.Id, true);

                _logger.LogInformation("Loaded Sector {Id}({Sector}) Territory = {Territory}", sector.Id, sector.Sector,
                    sector.TerritoryId);
            }
            catch (Exception e)
            {
                // On Failure... expire the sector quicker.
                // Maybe the server is under load
                _logger.LogError(e, "Failed to Load Sector {Id}({Sector})", sector.Id, sector.Sector);
                await _sectorInstanceRepository.SetLoadedAsync(sector.Id, true);
                await _sectorInstanceRepository.SetExpirationFromNowAsync(sector.Id, TimeSpan.FromMinutes(10));
            }
        }
    }

    public async Task ExecuteSectorCleanup()
    {
        try
        {
            await _sectorInstanceRepository.ExpireSectorsWithDeletedConstructHandles();
            await _constructHandleManager.TagAsDeletedConstructHandledThatAreDeletedConstructs();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to ExpireSectorsWithDeletedConstructHandles");
        }

        var expiredSectors = await _sectorInstanceRepository.FindExpiredAsync();

        foreach (var sector in expiredSectors)
        {
            var players = await _constructSpatial.FindPlayerLiveConstructsOnSector(sector.Sector);
            if (!sector.IsForceExpired(DateTime.UtcNow) && players.Any())
            {
                _logger.LogInformation("Players Nearby - Extended Expiration of {Sector} {SectorGuid}", sector.Sector,
                    sector.Id);
                await _sectorInstanceRepository.SetExpirationFromNowAsync(sector.Id, TimeSpan.FromMinutes(60));
                continue;
            }

            await _constructHandleManager.CleanupConstructHandlesInSectorAsync(sector.Sector);
            await Task.Delay(200);
        }

        await _sectorInstanceRepository.DeleteExpiredAsync();
        // await _constructHandleManager.CleanupConstructsThatFailedSectorCleanupAsync();

        _logger.LogInformation("Executed Sector Cleanup");
    }

    public async Task SetExpirationFromNow(Vec3 sector, TimeSpan span)
    {
        var instance = await _sectorInstanceRepository.FindBySector(sector);

        if (instance == null)
        {
            return;
        }

        await _sectorInstanceRepository.SetExpirationFromNowAsync(instance.Id, span);

        _logger.LogInformation(
            "Set Sector expiration for {Sector}({Id}) to {Minutes} from now",
            instance.Sector,
            instance.Id,
            span
        );
    }

    public async Task<SectorActivationOutcome> ForceActivateSector(Guid sectorId)
    {
        var sectorInstance = await _sectorInstanceRepository.FindById(sectorId);

        if (sectorInstance == null)
        {
            return SectorActivationOutcome.Failed($"Sector {sectorId} not found");
        }

        return await ActivateSector(sectorInstance);
    }

    public async Task ActivateEnteredSectors()
    {
        var sectorsToActivate = (await _sectorInstanceRepository
                .ScanForInactiveSectorsVisitedByPlayers(DistanceHelpers.OneSuInMeters * 10))
            .DistinctBy(x => x.Sector)
            .ToList();

        if (!sectorsToActivate.Any())
        {
            _logger.LogDebug("No sectors need startup");
            return;
        }

        foreach (var sectorInstance in sectorsToActivate)
        {
            await ActivateSector(sectorInstance);
        }
    }

    private async Task<SectorActivationOutcome> ActivateSector(SectorInstance sectorInstance)
    {
        var spatialHashRepository = serviceProvider.GetRequiredService<IConstructSpatialHashRepository>();
        var orleans = serviceProvider.GetOrleans();

        var constructs = (await spatialHashRepository.FindPlayerLiveConstructsOnSector(sectorInstance.Sector))
            .ToList();

        if (constructs.Count == 0)
        {
            return SectorActivationOutcome.Failed("No Player Constructs");
        }

        HashSet<ulong> playerIds = [];

        try
        {
            var queryPilotsTasks = constructs
                .Select(x => orleans.GetConstructInfoGrain(x)).Select(x => x.Get());

            playerIds = (await Task.WhenAll(queryPilotsTasks))
                .Select(x => x.mutableData.pilot)
                .Where(x => x.HasValue)
                .Select(x => x.Value.id)
                .ToHashSet();
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Failed to query player IDS for Sector Startup. Sector will startup without that information");
        }

        _logger.LogInformation(
            "Starting up sector F({Faction}) ({Sector}) encounter: '{Encounter}'",
            sectorInstance.FactionId,
            sectorInstance.Sector,
            sectorInstance.OnSectorEnterScript
        );

        return await ActivateSectorInternal(sectorInstance, playerIds, constructs.ToHashSet());
    }

    private async Task<SectorActivationOutcome> ActivateSectorInternal(
        SectorInstance sectorInstance,
        HashSet<ulong> playerIds,
        HashSet<ulong> constructIds
    )
    {
        var scriptService = serviceProvider.GetRequiredService<IScriptService>();
        var eventService = serviceProvider.GetRequiredService<IEventService>();
        var random = serviceProvider.GetRandomProvider().GetRandom();

        try
        {
            await scriptService.ExecuteScriptAsync(
                sectorInstance.OnSectorEnterScript,
                new ScriptContext(
                    serviceProvider,
                    sectorInstance.FactionId,
                    [],
                    sectorInstance.Sector,
                    sectorInstance.TerritoryId
                )
                {
                    PlayerIds = playerIds,
                    // TODO Properties for OnSectorEnterScript
                    // Properties = 
                }
            );

            await _sectorInstanceRepository.TagAsStartedAsync(sectorInstance.Id);
            await Task.Delay(200);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Failed to start encounter({Encounter}) at sector({Sector})",
                sectorInstance.OnSectorEnterScript,
                sectorInstance.Sector
            );
            
            return SectorActivationOutcome.Failed(e.Message);
        }

        try
        {
            ulong? playerId = null;
            if (playerIds.Count > 0)
            {
                playerId = random.PickOneAtRandom(playerIds);
            }

            await eventService.PublishAsync(
                new SectorActivatedEvent(
                    playerIds,
                    playerId,
                    sectorInstance.Sector,
                    random.PickOneAtRandom(constructIds),
                    playerIds.Count
                )
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to publish {Event}", nameof(SectorActivatedEvent));
        }
        
        return SectorActivationOutcome.Activated();
    }

    public async Task UpdateExpirationNames()
    {
        var constructHandleRepository = serviceProvider.GetRequiredService<IConstructHandleRepository>();
        var poiMap = await constructHandleRepository.GetPoiConstructExpirationTimeSpansAsync();
        var orleans = serviceProvider.GetOrleans();

        _logger.LogInformation("Update Expiration Names found: {Count}", poiMap.Count);

        using var db = serviceProvider.GetRequiredService<IPostgresConnectionFactory>().Create();
        db.Open();

        foreach (var kvp in poiMap)
        {
            try
            {
                var constructInfoGrain = orleans.GetConstructInfoGrain(kvp.Key);
                var constructInfo = await constructInfoGrain.Get();
                var constructName = constructInfo.rData.name;

                var pieces = constructName.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Length == 0)
                {
                    continue;
                }

                var firstPiece = pieces[0].Trim();
                var newName = $"{firstPiece} | [{(int)kvp.Value.TotalMinutes}m]";

                // TODO move this to a service or repo
                await db.ExecuteAsync("UPDATE public.construct SET name = @name WHERE id = @id", new
                {
                    name = newName,
                    id = (long)kvp.Key
                });

                _logger.LogDebug("Construct {Construct} Name Updated to: {Name}", kvp.Key, newName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update expiration name of {Construct}", kvp.Key);
            }
        }
    }

    private Task ExpireSector(SectorInstance instance)
    {
        return _sectorInstanceRepository.DeleteAsync(instance.Id);
    }
}