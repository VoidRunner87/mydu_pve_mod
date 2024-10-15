using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public class ProceduralQuestGeneratorService(IServiceProvider provider) : IProceduralQuestGeneratorService
{
    private readonly ILogger<ProceduralQuestGeneratorService> _logger =
        provider.CreateLogger<ProceduralQuestGeneratorService>();

    public async Task<GenerateQuestListOutcome> Generate(
        PlayerId playerId, 
        FactionId factionId,
        TerritoryId territoryId,
        int seed,
        int quantity)
    {
        var timeFactor = TimeUtility.GetTimeSnapped(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(10));
        var random = new Random(seed + (int)timeFactor);
        var result = new List<ProceduralQuestItem>();

        var questSeed = random.Next();
        var questType = random.PickOneAtRandom(QuestTypes.All());

        var factionTerritoryRepository = provider.GetRequiredService<IFactionTerritoryRepository>();
        var factionTerritoryMap = (await factionTerritoryRepository.GetAllByFactionAsync(factionId))
            .ToDictionary(
                k => k.TerritoryId,
                v => v
            );
        
        // remove param territory
        factionTerritoryMap.Remove(territoryId);

        if (factionTerritoryMap.Keys.Count == 0)
        {
            return GenerateQuestListOutcome.NoQuestsAvailable("No other faction territories available");
        }
        
        var territoryContainerRepository = provider.GetRequiredService<ITerritoryContainerRepository>();
        var fromContainerList = (await territoryContainerRepository.GetAll(territoryId)).ToList();

        if (fromContainerList.Count == 0)
        {
            return GenerateQuestListOutcome.NoQuestsAvailable("No pickup containers available");
        }
        
        var questPickupContainer = random.PickOneAtRandom(fromContainerList);

        var dropContainerTerritory = random.PickOneAtRandom(factionTerritoryMap.Keys);
        var dropContainerList = (await territoryContainerRepository.GetAll(dropContainerTerritory)).ToList();

        if (dropContainerList.Count == 0)
        {
            return GenerateQuestListOutcome.NoQuestsAvailable("No drop containers available");
        }
        
        var dropPickupContainer = random.PickOneAtRandom(dropContainerList);

        var questGuid = GuidUtility.Create(
            territoryId,
            $"{questType}-{factionId.Id}-{territoryId.Id}-{timeFactor}"
        );
        var pickupGuid = GuidUtility.Create(
            territoryId,
            $"{QuestTaskItemType.PickupItem}-{factionId.Id}-{territoryId.Id}-{timeFactor}"
        );
        var dropGuid = GuidUtility.Create(
            territoryId,
            $"{QuestTaskItemType.DropItem}-{factionId.Id}-{territoryId.Id}-{timeFactor}"
        );
        
        result.Add(
            new ProceduralQuestItem(
                questGuid,
                factionId,
                questType,
                questSeed,
                new List<QuestTaskItem>
                {
                    new(
                        pickupGuid,
                        "",
                        QuestTaskItemType.PickupItem,
                        new Vec3(),
                        null,
                        null,
                        new PickupItemTaskItemDefinition(questPickupContainer)
                    ),
                    new(
                        dropGuid,
                        "",
                        QuestTaskItemType.DropItem,
                        new Vec3(),
                        null,
                        null,
                        new DropItemTaskDefinition(dropPickupContainer)
                    )
                }
            )
        );
        
        return GenerateQuestListOutcome.WithAvailableQuests(result);
    }
}