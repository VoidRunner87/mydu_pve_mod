using System;
using System.Diagnostics;
using System.Linq;
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

namespace Mod.DynamicEncounters;

public class SectorLoop : ModBase
{
    public override async Task Loop()
    {
        var logger = ServiceProvider.CreateLogger<SectorLoop>();
        
        while (true)
        {
            await Task.Delay(5000);

            try
            {
                var featureService = ServiceProvider.GetRequiredService<IFeatureReaderService>();
                var isEnabled = await featureService.GetEnabledValue<SectorLoop>(false);

                if (isEnabled)
                {
                    await ExecuteAction();
                }
                
                RecordHeartBeat();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to execute {Name}", nameof(SectorLoop));
            }
        }
    }

    private async Task ExecuteAction()
    {
        var sw = new Stopwatch();
        sw.Start();

        var logger = ServiceProvider.CreateLogger<SectorLoop>();
        
        var factionRepository = ServiceProvider.GetRequiredService<IFactionRepository>();
        var sectorPoolManager = ServiceProvider.GetRequiredService<ISectorPoolManager>();
        
        await sectorPoolManager.ExecuteSectorCleanup()
            .OnError(exception => { logger.LogError(exception, "Failed to Execute Sector Cleanup"); });

        var factionSectorPrepTasks = (await factionRepository.GetAllAsync())
            .Select(PrepareFactionSector);
        await Task.WhenAll(factionSectorPrepTasks);

        await sectorPoolManager.LoadUnloadedSectors();
        await sectorPoolManager.ActivateEnteredSectors();

        logger.LogInformation("Sector Loop Action took {Time}ms", sw.ElapsedMilliseconds);
    }

    private async Task PrepareFactionSector(FactionItem faction)
    {
        var logger = ServiceProvider.CreateLogger<SectorLoop>();
        logger.LogDebug("Preparing Sector for {Faction}", faction.Name);

        // TODO sector encounters becomes tied to a territory
        // a territory has center, max and min radius
        // a territory is owned by a faction
        // a territory is a point on a map - not constructs nor DU's territories - perhaps name it differently
        var sectorEncountersRepository = ServiceProvider.GetRequiredService<ISectorEncounterRepository>();
        // var encounters = (await sectorEncountersRepository.FindActiveByFactionAsync(faction.Id))
        //     .ToList();
        //
        // if (encounters.Count == 0)
        // {
        //     logger.LogDebug("No Encounters for Faction: {Faction}({Id})", faction.Name, faction.Id);
        //     return;
        // }

        var factionTerritoryRepository = ServiceProvider.GetRequiredService<IFactionTerritoryRepository>();
        var factionTerritories = await factionTerritoryRepository.GetAllByFactionAsync(faction.Id);

        // var generationArgs = new SectorGenerationArgs
        // {
        //     Encounters = encounters,
        //     Quantity = faction.Properties.SectorPoolCount,
        //     FactionId = faction.Id,
        //     Tag = faction.Tag,
        // };

        var sectorPoolManager = ServiceProvider.GetRequiredService<ISectorPoolManager>();
        // await sectorPoolManager.GenerateSectors(generationArgs);

        foreach (var ft in factionTerritories)
        {
            var encounters = (await sectorEncountersRepository.FindActiveByFactionTerritoryAsync(ft.FactionId, ft.TerritoryId))
                .ToList();

            if (encounters.Count == 0)
            {
                logger.LogInformation("No Encounters for Faction: {Faction}({Id}) Territory({Territory})", faction.Name, faction.Id, ft.TerritoryId);
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
                logger.LogError(e, "Failed to generate faction({F}) territory({T}) sectors", args.FactionId, args.TerritoryId);                
            }
        }
    }
}