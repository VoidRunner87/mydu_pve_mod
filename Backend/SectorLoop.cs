using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
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
        
        await _sectorPoolManager.ExecuteSectorCleanup();
        
        var factionSectorPrepTasks = (await factionRepository.GetAllAsync())
            .Select(PrepareSectorByFaction);
        await Task.WhenAll(factionSectorPrepTasks);
        
        await _sectorPoolManager.LoadUnloadedSectors();
        await _sectorPoolManager.ActivateEnteredSectors();

        _logger.LogDebug("Action took {Time}ms", sw.ElapsedMilliseconds);
    }
    
    private async Task PrepareSectorByFaction(FactionItem faction)
    {
        // TODO sector encounters becomes tied to a territory
        // a territory has center, max and min radius
        // a territory is owned by a faction
        // a territory is a point on a map - not constructs nor DU's territories - perhaps name it differently
        var sectorEncountersRepository = ServiceProvider.GetRequiredService<ISectorEncounterRepository>();
        var encounters = (await sectorEncountersRepository.FindActiveByFactionAsync(faction.Id))
            .ToList();
        
        if (encounters.Count == 0)
        {
            _logger.LogDebug("No Encounters for Faction: {Faction}({Id})", faction.Name, faction.Id);
            return;
        }

        var generationArgs = new SectorGenerationArgs
        {
            Encounters = encounters,
            Quantity = faction.Properties.SectorPoolCount,
            FactionId = faction.Id,
            Tag = faction.Tag
        };
        
        await _sectorPoolManager.GenerateSectors(generationArgs);
    }
}