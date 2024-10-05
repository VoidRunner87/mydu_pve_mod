using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
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

    public override Task Start()
    {
        return Task.WhenAll([
            base.Start(),
            UpdateExpirationNames(),
            ManageSectors()
        ]);
    }

    private Task UpdateExpirationNames()
    {
        var taskCompletionSource = new TaskCompletionSource();
        var logger = ServiceProvider.CreateLogger<SectorLoop>();
        
        try
        {
            var sectorPoolManager = ServiceProvider.GetRequiredService<ISectorPoolManager>();
            
            var updateExpirationNameTimer = new Timer(TimeSpan.FromSeconds(30));
            updateExpirationNameTimer.Elapsed += async (sender, args) =>
            {
                var featureService = ServiceProvider.GetRequiredService<IFeatureReaderService>();

                if (await featureService.GetEnabledValue<SectorLoop>(false))
                {
                    await sectorPoolManager.UpdateExpirationNames();
                }
            };
            updateExpirationNameTimer.Start();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to execute {Name} Timer", nameof(SectorLoop));
        }

        return taskCompletionSource.Task;
    }

    public async Task ManageSectors()
    {
        var logger = ServiceProvider.CreateLogger<SectorLoop>();
        
        while (true)
        {
            await Task.Delay(3000);

            try
            {
                var featureService = ServiceProvider.GetRequiredService<IFeatureReaderService>();
                var isEnabled = await featureService.GetEnabledValue<SectorLoop>(false);

                if (isEnabled)
                {
                    await ExecuteAction();
                }
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
            .Select(PrepareSector);
        await Task.WhenAll(factionSectorPrepTasks);

        await sectorPoolManager.LoadUnloadedSectors();
        await sectorPoolManager.ActivateEnteredSectors();

        logger.LogInformation("Sector Loop Action took {Time}ms", sw.ElapsedMilliseconds);
    }

    private async Task PrepareSector(FactionItem faction)
    {
        var logger = ServiceProvider.CreateLogger<SectorLoop>();
        logger.LogDebug("Preparing Sector for {Faction}", faction.Name);

        // TODO sector encounters becomes tied to a territory
        // a territory has center, max and min radius
        // a territory is owned by a faction
        // a territory is a point on a map - not constructs nor DU's territories - perhaps name it differently
        var sectorEncountersRepository = ServiceProvider.GetRequiredService<ISectorEncounterRepository>();
        var encounters = (await sectorEncountersRepository.FindActiveByFactionAsync(faction.Id))
            .ToList();

        if (encounters.Count == 0)
        {
            logger.LogDebug("No Encounters for Faction: {Faction}({Id})", faction.Name, faction.Id);
            return;
        }

        var generationArgs = new SectorGenerationArgs
        {
            Encounters = encounters,
            Quantity = faction.Properties.SectorPoolCount,
            FactionId = faction.Id,
            Tag = faction.Tag
        };

        var sectorPoolManager = ServiceProvider.GetRequiredService<ISectorPoolManager>();
        await sectorPoolManager.GenerateSectors(generationArgs);
    }
}