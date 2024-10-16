﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public class ProceduralTransportMissionGeneratorService(IServiceProvider provider)
    : IProceduralTransportMissionGeneratorService
{
    private readonly ILogger<ProceduralQuestGeneratorService> _logger =
        provider.CreateLogger<ProceduralQuestGeneratorService>();

    public async Task<ProceduralQuestOutcome> GenerateAsync(
        PlayerId playerId,
        FactionId factionId,
        TerritoryId territoryId,
        int seed
    )
    {
        var factionRepository = provider.GetRequiredService<IFactionRepository>();
        var faction = await factionRepository.FindAsync(factionId);

        if (faction == null)
        {
            return ProceduralQuestOutcome.Failed($"Faction {factionId.Id} not found");
        }

        var timeFactor = TimeUtility.GetTimeSnapped(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(10));
        var random = new Random(seed);

        var questSeed = random.Next();
        const string questType = QuestTypes.Transport;

        var factionTerritoryRepository = provider.GetRequiredService<IFactionTerritoryRepository>();
        
        var territoryMap = (await factionTerritoryRepository.GetAll())
            .DistinctBy(v => v.TerritoryId)
            .ToDictionary(
                k => k.TerritoryId,
                v => v
            );

        // remove param territory
        territoryMap.Remove(territoryId);

        if (territoryMap.Keys.Count == 0)
        {
            return ProceduralQuestOutcome.Failed("No other faction territories available");
        }

        var territoryContainerRepository = provider.GetRequiredService<ITerritoryContainerRepository>();
        var fromContainerList = (await territoryContainerRepository.GetAll(territoryId)).ToList();

        if (fromContainerList.Count == 0)
        {
            return ProceduralQuestOutcome.Failed("No pickup containers available");
        }

        var questPickupContainer = random.PickOneAtRandom(fromContainerList);

        var dropContainerTerritory = random.PickOneAtRandom(territoryMap.Keys);
        var dropContainerList = (await territoryContainerRepository.GetAll(dropContainerTerritory)).ToList();

        if (dropContainerList.Count == 0)
        {
            return ProceduralQuestOutcome.Failed("No drop containers available");
        }

        var dropContainer = random.PickOneAtRandom(dropContainerList);

        var pickupGuid = GuidUtility.Create(
            territoryId,
            $"{QuestTaskItemType.Pickup}-{factionId.Id}-{territoryId.Id}-{timeFactor}"
        );
        var dropGuid = GuidUtility.Create(
            territoryId,
            $"{QuestTaskItemType.Deliver}-{factionId.Id}-{dropContainerTerritory}-{timeFactor}"
        );
        var questGuid = GuidUtility.Create(
            territoryId,
            $"{questType}-{factionId.Id}-{territoryId.Id}-{pickupGuid}-{dropGuid}-{timeFactor}"
        );

        var constructService = provider.GetRequiredService<IConstructService>();
        var pickupConstructInfo = await constructService.GetConstructInfoAsync(questPickupContainer.ConstructId);
        var dropConstructInfo = await constructService.GetConstructInfoAsync(dropContainer.ConstructId);

        if (!pickupConstructInfo.ConstructExists || pickupConstructInfo.Info == null)
        {
            return ProceduralQuestOutcome.Failed(
                $"Pickup Construct '{questPickupContainer.ConstructId}' doesn't exist");
        }

        if (!dropConstructInfo.ConstructExists || dropConstructInfo.Info == null)
        {
            return ProceduralQuestOutcome.Failed($"Drop Construct '{dropContainer.ConstructId}' doesn't exist");
        }

        var pickupTaskTitle = $"Pickup items at: {pickupConstructInfo.Info.rData.name}";
        var dropOffTaskTitle = $"Deliver items to: {dropConstructInfo.Info.rData.name}";

        var distanceSu = (pickupConstructInfo.Info.rData.position - dropConstructInfo.Info.rData.position).Size()
                         / DistanceHelpers.OneSuInMeters;

        var titles = new List<string>
        {
            $"Supply Run from {pickupConstructInfo.Info.rData.name} to {dropConstructInfo.Info.rData.name} [{distanceSu:N2}su]",
            $"Transport of Goods from {pickupConstructInfo.Info.rData.name} to {dropConstructInfo.Info.rData.name} [{distanceSu:N2}su]",
            $"Delivery to {dropConstructInfo.Info.rData.name} [{distanceSu:N2}su]",
        };

        var multiplier = 1;
        if (dropConstructInfo.Info.kind == ConstructKind.STATIC)
        {
            multiplier++;
        }

        if (pickupConstructInfo.Info.kind == ConstructKind.STATIC)
        {
            multiplier++;
        }
        
        var title = random.PickOneAtRandom(titles);
        var quantaReward = (long)(distanceSu * 10000d * 100d * 1.45 * multiplier);
        var influenceReward = 1;

        return ProceduralQuestOutcome.Created(
            new ProceduralQuestItem(
                questGuid,
                factionId,
                questType,
                questSeed,
                title,
                new ProceduralQuestProperties
                {
                    RewardTextList =
                    [
                        $"{quantaReward / 100:N2}h",
                        $"Influence with {faction.Name} +{influenceReward}"
                    ],
                    QuantaReward = quantaReward,
                    InfluenceReward =
                    {
                        { factionId, influenceReward }
                    },
                    ExpiresAt = DateTime.Now + TimeSpan.FromHours(3)
                },
                new List<QuestTaskItem>
                {
                    new(
                        pickupGuid,
                        pickupTaskTitle,
                        QuestTaskItemType.Pickup,
                        QuestTaskItemStatus.InProgress,
                        pickupConstructInfo.Info.rData.position,
                        new ScriptActionItem
                        {
                            Type = "assert-task-completion",
                            FactionId = factionId,
                            TerritoryId = territoryId,
                            ConstructId = pickupConstructInfo.Info.rData.constructId,
                            Properties =
                            {
                                { "questId", questGuid },
                                { "questTaskId", pickupGuid }
                            }
                        },
                        new PickupItemTaskItemDefinition(questPickupContainer)
                    ),
                    new(
                        dropGuid,
                        dropOffTaskTitle,
                        QuestTaskItemType.Deliver,
                        QuestTaskItemStatus.InProgress,
                        dropConstructInfo.Info.rData.position,
                        new ScriptActionItem
                        {
                            Type = "assert-task-completion",
                            FactionId = factionId,
                            TerritoryId = territoryId,
                            ConstructId = pickupConstructInfo.Info.rData.constructId,
                            Properties =
                            {
                                { "questId", questGuid },
                                { "questTaskId", dropGuid }
                            }
                        },
                        new DropItemTaskDefinition(dropContainer)
                    )
                }
            )
        );
    }
}