using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Scenegraph;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Helpers;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public class ProceduralReverseTransportMissionGeneratorService(IServiceProvider provider)
    : IProceduralReverseTransportMissionGeneratorService
{
    private readonly IConstructService _constructService =
        provider.GetRequiredService<IConstructService>();
    
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

        var timeFactor = TimeUtility.GetTimeSnapped(DateTimeOffset.UtcNow, MissionProceduralGenerationConfig.TransportMissionTimeFactor);
        var random = new Random(seed);

        var questSeed = random.Next();
        const string questType = QuestTypes.ReverseTransport;

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
        var toContainerList = (await territoryContainerRepository.GetAll(territoryId)).ToList();

        if (toContainerList.Count == 0)
        {
            return ProceduralQuestOutcome.Failed("No delivery containers available");
        }

        var deliveryContainer = random.PickOneAtRandom(toContainerList);

        var fromContainerTerritory = random.PickOneAtRandom(territoryMap.Keys);
        var fromContainerList = (await territoryContainerRepository.GetAll(fromContainerTerritory)).ToList();

        if (fromContainerList.Count == 0)
        {
            return ProceduralQuestOutcome.Failed("No pickup containers available");
        }

        var fromContainer = random.PickOneAtRandom(fromContainerList);

        var toGuid = GuidUtility.Create(
            territoryId,
            $"{playerId}-{QuestTaskItemType.Deliver}-{factionId.Id}-{territoryId.Id}-{timeFactor}"
        );
        var fromGuid = GuidUtility.Create(
            territoryId,
            $"{playerId}-{QuestTaskItemType.Pickup}-{factionId.Id}-{fromContainerTerritory}-{timeFactor}"
        );
        var questGuid = GuidUtility.Create(
            territoryId,
            $"{questType}-{factionId.Id}-{territoryId.Id}-{toGuid}-{fromGuid}-{timeFactor}"
        );

        var constructService = provider.GetRequiredService<IConstructService>();
        var deliverConstructInfo = await constructService.GetConstructInfoAsync(deliveryContainer.ConstructId);
        var pickupConstructInfo = await constructService.GetConstructInfoAsync(fromContainer.ConstructId);

        if (!pickupConstructInfo.ConstructExists || pickupConstructInfo.Info == null)
        {
            return ProceduralQuestOutcome.Failed(
                $"Pickup Construct '{deliveryContainer.ConstructId}' doesn't exist");
        }

        if (!deliverConstructInfo.ConstructExists || deliverConstructInfo.Info == null)
        {
            return ProceduralQuestOutcome.Failed($"Drop Construct '{fromContainer.ConstructId}' doesn't exist");
        }

        var transportMissionTemplateProvider = provider.GetRequiredService<ITransportMissionTemplateProvider>();
        var missionTemplate = await transportMissionTemplateProvider.GetMissionTemplate(random.Next());
        missionTemplate = missionTemplate
            .SetPickupConstructName(pickupConstructInfo.Info.rData.name)
            .SetDeliverConstructName(deliverConstructInfo.Info.rData.name);

        var sceneGraph = provider.GetRequiredService<IScenegraph>();
        var deliveryPos = await sceneGraph.GetConstructCenterWorldPosition(deliveryContainer.ConstructId);
        var pickupPos = await sceneGraph.GetConstructCenterWorldPosition(fromContainer.ConstructId);

        var distanceMeters = (pickupPos - deliveryPos).Size();
        var distanceSu = distanceMeters / DistanceHelpers.OneSuInMeters;

        var multiplier = 1;
        if (deliverConstructInfo.Info.kind == ConstructKind.STATIC)
        {
            multiplier++;
        }

        if (pickupConstructInfo.Info.kind == ConstructKind.STATIC)
        {
            multiplier++;
        }
        
        var pickupInSafeZone = await _constructService.IsInSafeZone(pickupConstructInfo.Info.rData.constructId);
        var dropInSafeZone = await _constructService.IsInSafeZone(deliverConstructInfo.Info.rData.constructId);
        var isSafe = pickupInSafeZone && dropInSafeZone;

        var quantaMultiplier = MissionProceduralGenerationConfig.ReverseTransportMultiplier;
        var unsafeMultiplier = isSafe ? 1 : MissionProceduralGenerationConfig.UnsafeMultiplier;
        var quantaReward = (long)(distanceSu * 10000d * 100d * quantaMultiplier * multiplier * unsafeMultiplier);
        var influenceReward = 1;

        var kergonQuantity = new LitreQuantity(3000);
        
        return ProceduralQuestOutcome.Created(
            new ProceduralQuestItem(
                questGuid,
                factionId,
                questType,
                questSeed,
                missionTemplate.Title,
                isSafe,
                DistanceHelpers.OneSuInMeters / 4d,
                new ProceduralQuestProperties
                {
                    RewardTextList =
                    [
                        $"{quantaReward / 100:N2}h",
                        $"Kergon X1: {kergonQuantity.GetRawQuantity()}L",
                        $"Influence with {faction.Name} +{influenceReward}"
                    ],
                    QuantaReward = quantaReward,
                    InfluenceReward =
                    {
                        { factionId, influenceReward }
                    },
                    ExpiresAt = DateTime.UtcNow + TimeSpan.FromHours(3),
                    ItemRewardMap =
                    {
                        {"Kergon1", kergonQuantity.ToQuantity()}
                    },
                    DistanceMeters = distanceMeters,
                    DistanceSu = distanceMeters
                },
                new List<QuestTaskItem>
                {
                    new(
                        new QuestTaskId(
                            questGuid,
                            Guid.NewGuid()
                        ),
                        missionTemplate.PickupMessage,
                        QuestTaskItemType.Pickup,
                        QuestTaskItemStatus.InProgress,
                        pickupPos,
                        null,
                        new ScriptActionItem
                        {
                            Type = "assert-task-completion",
                            FactionId = factionId,
                            TerritoryId = territoryId,
                            ConstructId = pickupConstructInfo.Info.rData.constructId,
                            Properties =
                            {
                                { "questId", questGuid },
                                { "questTaskId", toGuid }
                            }
                        },
                        new PickupItemTaskItemDefinition(
                            fromContainer,
                            missionTemplate.Items
                        )
                    ),
                    new(
                        new QuestTaskId(
                            questGuid,
                            Guid.NewGuid()
                        ),
                        missionTemplate.DeliverMessage,
                        QuestTaskItemType.Deliver,
                        QuestTaskItemStatus.InProgress,
                        deliveryPos,
                        null,
                        new ScriptActionItem
                        {
                            Type = "assert-task-completion",
                            FactionId = factionId,
                            TerritoryId = territoryId,
                            ConstructId = deliverConstructInfo.Info.rData.constructId,
                            Properties =
                            {
                                { "questId", questGuid },
                                { "questTaskId", fromGuid }
                            }
                        },
                        new DeliverItemTaskDefinition(
                            deliveryContainer,
                            missionTemplate.Items
                                .Select(x => new QuestElementQuantityRef(
                                    x.ElementId,
                                    x.ElementTypeName, 
                                    -x.Quantity
                                ))
                        )
                    )
                }
            )
        );
    }
}