using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Repository;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class SectorLoop : ModBase
{
    private ILogger<SectorLoop> _logger;
    private ISectorPoolManager _sectorPoolManager;
    private const string SectorsToGenerateFeatureName = "SectorsToGenerate";

    public override async Task Loop()
    {
        _logger = ServiceProvider.CreateLogger<SectorLoop>();

        var featureService = ServiceProvider.GetRequiredService<IFeatureReaderService>();
        var spawnerService = ServiceProvider.GetRequiredService<IScriptService>();
        _sectorPoolManager = ServiceProvider.GetRequiredService<ISectorPoolManager>();

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
            _logger.LogError(e, "Failed to execute {Name}", nameof(SectorLoop));
            // TODO implement alerting on too many failures
        }
    }

    private async Task ExecuteAction()
    {
        var sw = new Stopwatch();
        sw.Start();

        var factionRepository = ServiceProvider.GetRequiredService<IFactionRepository>();
        var featuresService = ServiceProvider.GetRequiredService<IFeatureReaderService>();
        var sectorsToGenerate = await featuresService.GetIntValueAsync(SectorsToGenerateFeatureName, 10);
        
        await PrepareSector(SectorEncounterTags.Pooled, sectorsToGenerate);

        var factionSectorPrepTasks = (await factionRepository.GetAllAsync())
            .Select(f => PrepareSector(f.Id, f.Properties.SectorPoolCount));
        await Task.WhenAll(factionSectorPrepTasks);
        
        await _sectorPoolManager.LoadUnloadedSectors();
        await _sectorPoolManager.ActivateEnteredSectors();

        _logger.LogDebug("Action took {Time}ms", sw.ElapsedMilliseconds);
    }

    private async Task PrepareSector(string tag, int sectorsToGenerate)
    {
        // sector encounters becomes tied to a territory
        // territory has center, max and min radius
        // territory is owned by a faction
        // territory is a point on a map - not constructs nor DU's territories - perhaps name it differently
        var sectorEncountersRepository = ServiceProvider.GetRequiredService<ISectorEncounterRepository>();
        var encounters = (await sectorEncountersRepository.FindActiveTaggedAsync(tag))
            .ToList();
        
        if (encounters.Count == 0)
        {
            _logger.LogWarning("No Encounters for Tag: {Tag}", tag);
            return;
        }

        var generationArgs = new SectorGenerationArgs
        {
            Encounters = encounters,
            Quantity = sectorsToGenerate,
            Tag = tag
        };
        
        await _sectorPoolManager.ExecuteSectorCleanup(generationArgs);
        await _sectorPoolManager.GenerateSectors(generationArgs);
    }
}