using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class SectorLoop(IThreadManager tm, CancellationToken ct) : ThreadHandle(ThreadId.Sector, tm, ct)
{
    public override async Task Tick()
    {
        var logger = ModBase.ServiceProvider.CreateLogger<SectorLoop>();

        try
        {
            var featureService = ModBase.ServiceProvider.GetRequiredService<IFeatureReaderService>();
            var isEnabled = await featureService.GetEnabledValue<SectorLoop>(false);

            if (isEnabled)
            {
                await ExecuteAction();
            }

            ReportHeartbeat();

            Thread.Sleep(TimeSpan.FromSeconds(10));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to execute {Name}", nameof(SectorLoop));
        }
    }

    private async Task ExecuteAction()
    {
        var sw = new Stopwatch();
        sw.Start();

        var logger = ModBase.ServiceProvider.CreateLogger<SectorLoop>();

        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            { "Thread", Environment.CurrentManagedThreadId }
        });

        var factionRepository = ModBase.ServiceProvider.GetRequiredService<IFactionRepository>();
        var sectorPoolManager = ModBase.ServiceProvider.GetRequiredService<ISectorPoolManager>();

        await sectorPoolManager.ExecuteSectorCleanup()
            .OnError(exception => { logger.LogError(exception, "Failed to Execute Sector Cleanup"); });

        var factionSectorPrepTasks = (await factionRepository.GetAllAsync())
            .Select(PrepareFactionSector);
        await Task.WhenAll(factionSectorPrepTasks);

        await sectorPoolManager.LoadUnloadedSectors();
        await sectorPoolManager.ActivateEnteredSectors();

        logger.LogDebug("Sector Loop Action took {Time}ms", sw.ElapsedMilliseconds);
    }

    private async Task PrepareFactionSector(FactionItem faction)
    {
        var logger = ModBase.ServiceProvider.CreateLogger<SectorLoop>();
        logger.LogDebug("Preparing Sector for {Faction}", faction.Name);

        // TODO sector encounters becomes tied to a territory
        // a territory has center, max and min radius
        // a territory is owned by a faction
        // a territory is a point on a map - not constructs nor DU's territories - perhaps name it differently
        var sectorEncountersRepository = ModBase.ServiceProvider.GetRequiredService<ISectorEncounterRepository>();

        var factionTerritoryRepository = ModBase.ServiceProvider.GetRequiredService<IFactionTerritoryRepository>();
        var factionTerritories = await factionTerritoryRepository.GetAllByFactionAsync(faction.Id);

        var sectorPoolManager = ModBase.ServiceProvider.GetRequiredService<ISectorPoolManager>();

        foreach (var ft in factionTerritories)
        {
            var encounters =
                (await sectorEncountersRepository.FindActiveByFactionTerritoryAsync(ft.FactionId, ft.TerritoryId))
                .ToList();

            if (encounters.Count == 0)
            {
                logger.LogDebug("No Encounters for Faction: {Faction}({Id}) Territory({Territory})", faction.Name,
                    faction.Id, ft.TerritoryId);
                continue;
            }

            var args = new SectorGenerationArgs
            {
                Encounters = encounters,
                Quantity = ft.SectorCount,
                FactionId = faction.Id,
                Tag = faction.Tag,
                TerritoryId = ft.TerritoryId,
            };

            try
            {
                await sectorPoolManager.GenerateTerritorySectors(args);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to generate faction({F}) territory({T}) sectors", args.FactionId,
                    args.TerritoryId);
            }
        }
    }
}