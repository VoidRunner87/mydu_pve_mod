using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Repository;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class SectorLoop : ModBase
{
    private const string SectorsToGenerateFeatureName = "SectorsToGenerate";
    
    public override async Task Loop()
    {
        var provider = ServiceProvider;
        var logger = provider.CreateLogger<SectorLoop>();

        var featureService = provider.GetRequiredService<IFeatureReaderService>();
        
        var spawnerService = ServiceProvider.GetRequiredService<IScriptService>();

        await spawnerService.LoadAllFromDatabase();
        
        try
        {
            while (true)
            {
                await Task.Delay(3000);
                var isEnabled = await featureService.GetEnabledValue<SectorLoop>(false);

                if (isEnabled)
                {
                    await ExecuteAction();
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to execute {Name}", nameof(SectorLoop));
            // TODO implement alerting on too many failures
        }
    }

    private async Task ExecuteAction()
    {
        var logger = ServiceProvider.CreateLogger<SectorLoop>();
        var featuresService = ServiceProvider.GetRequiredService<IFeatureReaderService>();
        var sectorsToGenerate = await featuresService.GetIntValueAsync(SectorsToGenerateFeatureName, 10);

        var sw = new Stopwatch();
        sw.Start();

        var sectorEncountersRepository = ServiceProvider.GetRequiredService<ISectorEncounterRepository>();
        var encounters = await sectorEncountersRepository
            .FindActiveTaggedAsync(SectorEncounterTags.Pooled);

        var generationArgs = new SectorGenerationArgs
        {
            Encounters = encounters,
            Quantity = sectorsToGenerate
        };

        var sectorPoolManager = ServiceProvider.GetRequiredService<ISectorPoolManager>();

        await sectorPoolManager.ExecuteSectorCleanup(Bot, generationArgs);
        await sectorPoolManager.GenerateSectors(generationArgs);
        await sectorPoolManager.LoadUnloadedSectors(Bot);
        await sectorPoolManager.ActivateEnteredSectors(Bot);

        logger.LogDebug("Action took {Time}ms", sw.ElapsedMilliseconds);
    }
}