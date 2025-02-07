using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Scenegraph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Helpers;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Market.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public class ProceduralLootBasedMissionGeneratorService(IServiceProvider provider)
    : IProceduralLootBasedMissionGeneratorService
{
    private readonly ILogger<ProceduralLootBasedMissionGeneratorService> _logger =
        provider.CreateLogger<ProceduralLootBasedMissionGeneratorService>();

    private readonly IConstructService _constructService =
        provider.GetRequiredService<IConstructService>();

    private readonly ILootGeneratorService _lootGeneratorService =
        provider.GetRequiredService<ILootGeneratorService>();

    private readonly IRecipePriceCalculator _recipePriceCalculator =
        provider.GetRequiredService<IRecipePriceCalculator>();

    private readonly IFeatureReaderService _featureReaderService =
        provider.GetRequiredService<IFeatureReaderService>();

    private readonly IGameplayBank _bank = provider.GetGameplayBank();

    public async Task<ProceduralQuestOutcome> GenerateAsync(
        PlayerId playerId,
        FactionId factionId,
        TerritoryId territoryId,
        int seed
    )
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            { nameof(playerId), playerId },
            { nameof(factionId), factionId },
            { nameof(territoryId), territoryId },
        });

        var priceMap = await _recipePriceCalculator.GetItemPriceMap();
        var factionRepository = provider.GetRequiredService<IFactionRepository>();
        var faction = await factionRepository.FindAsync(factionId);

        if (faction == null)
        {
            return ProceduralQuestOutcome.Failed($"Faction {factionId.Id} not found");
        }

        var timeFactor =
            TimeUtility.GetTimeSnapped(DateTimeOffset.UtcNow, MissionProceduralGenerationConfig.OrderMissionTimeFactor);
        var random = new Random(seed);

        var questSeed = random.Next();
        const string questType = QuestTypes.Order;

        var lootItems = await _lootGeneratorService.GenerateGrouped(
            new LootGenerationArgs
            {
                Tags = ["quest"],
                MaxBudget = random.Next(10, 1000),
                Seed = questSeed,
                Operator = TagOperator.AllTags
            });

        if (lootItems.Count == 0)
        {
            return ProceduralQuestOutcome.Failed("No loot items tagged ['quest']");
        }

        var (lootName, lootItem) = random.PickOneAtRandom(lootItems);

        var entries = lootItem.GetEntries().ToArray();
        random.Shuffle(entries);
        entries = entries.Take(10).ToArray();

        var calculationResult = CalculateLootPrice(priceMap, entries);
        var totalPrice = calculationResult.TotalPrice;
        var questItems = calculationResult.QuestItems;

        var lootReward = await _lootGeneratorService.GenerateAsync(
            new LootGenerationArgs
            {
                Tags = lootItem.Tags,
                MaxBudget = random.Next(50, 100),
                Seed = random.Next(),
                Operator = TagOperator.AllTags
            });

        var rewardEntries = lootReward.GetEntries().ToArray();
        random.Shuffle(rewardEntries);
        rewardEntries = rewardEntries.Take(10).ToArray();

        var rewardCalculationResult = CalculateLootPrice(priceMap, rewardEntries);
        var rewardTotalPrice = rewardCalculationResult.TotalPrice;
        var rewardQuestItems = rewardCalculationResult.QuestItems;
        
        var factionTerritoryRepository = provider.GetRequiredService<IFactionTerritoryRepository>();

        var territoryMap = (await factionTerritoryRepository.GetAll())
            .DistinctBy(v => v.TerritoryId)
            .ToDictionary(
                k => k.TerritoryId,
                v => v
            );

        if (territoryMap.Keys.Count == 0)
        {
            return ProceduralQuestOutcome.Failed("No faction territories available");
        }

        var territoryContainerRepository = provider.GetRequiredService<ITerritoryContainerRepository>();

        var dropContainerTerritory = random.PickOneAtRandom(territoryMap.Keys);
        var dropContainerList = (await territoryContainerRepository.GetAll(dropContainerTerritory)).ToList();

        if (dropContainerList.Count == 0)
        {
            return ProceduralQuestOutcome.Failed(
                $"No drop containers available for order mission '{dropContainerTerritory}'");
        }

        var dropContainer = random.PickOneAtRandom(dropContainerList);

        var dropGuid = GuidUtility.Create(
            territoryId,
            $"{playerId}-{QuestTaskItemType.DeliverUnrestricted}-{factionId.Id}-{territoryId.Id}-{timeFactor}"
        );
        var questGuid = GuidUtility.Create(
            territoryId,
            $"{questType}-{lootName}-{factionId.Id}-{territoryId.Id}-{dropGuid}-{timeFactor}"
        );

        var constructService = provider.GetRequiredService<IConstructService>();
        var dropConstructInfo = await constructService.GetConstructInfoAsync(dropContainer.ConstructId);

        if (!dropConstructInfo.ConstructExists || dropConstructInfo.Info == null)
        {
            return ProceduralQuestOutcome.Failed($"Drop Construct '{dropContainer.ConstructId}' doesn't exist");
        }

        var sceneGraph = provider.GetRequiredService<IScenegraph>();
        var deliveryPos = await sceneGraph.GetConstructCenterWorldPosition(dropContainer.ConstructId);

        var dropInSafeZone = await _constructService.IsInSafeZone(dropConstructInfo.Info.rData.constructId);

        var safeMultiplier = await _featureReaderService.GetDoubleValueAsync("OrderMissionSafeMultiplier", 0.8d);
        var pvpMultiplier = await _featureReaderService.GetDoubleValueAsync("OrderMissionPvpMultiplier", 1.5d);
        
        var quantaReward = totalPrice * (dropInSafeZone ? safeMultiplier : pvpMultiplier) - rewardTotalPrice;
        quantaReward = Math.Clamp(quantaReward, 0, Math.Abs(quantaReward));

        var lootRewardTextItems = new List<string> { $"{quantaReward / 100:N2}h" };

        foreach (var reward in lootReward.GetEntries())
        {
            var definition = _bank.GetDefinition(reward.ItemName);
            if (definition == null) continue;
            var baseObj = definition.BaseObject;

            lootRewardTextItems.Add($"{reward.Quantity.GetRawQuantity()}x {baseObj.DisplayName}");
        }

        return ProceduralQuestOutcome.Created(
            new ProceduralQuestItem(
                questGuid,
                factionId,
                questType,
                questSeed,
                $"Order of {lootName} for {dropConstructInfo.Info.rData.name}",
                dropInSafeZone,
                -1,
                new ProceduralQuestProperties
                {
                    RewardTextList = lootRewardTextItems,
                    QuantaReward = (long)quantaReward,
                    InfluenceReward =
                    {
                        { factionId, 1000 }
                    },
                    ExpiresAt = DateTime.UtcNow + TimeSpan.FromHours(3),
                    ItemRewardMap = lootReward.GetEntries()
                        .ToDictionary(k => k.ItemName, v => v.Quantity.ToQuantity()),
                    DistanceMeters = 0,
                    DistanceSu = 0
                },
                new List<QuestTaskItem>
                {
                    new(
                        new QuestTaskId(
                            questGuid,
                            Guid.NewGuid()
                        ),
                        $"Deliver items to {dropConstructInfo.Info.rData.name}",
                        QuestTaskItemType.DeliverUnrestricted,
                        QuestTaskItemStatus.InProgress,
                        deliveryPos,
                        null,
                        new ScriptActionItem
                        {
                            Type = "assert-task-completion",
                            FactionId = factionId,
                            TerritoryId = territoryId,
                            ConstructId = dropConstructInfo.Info.rData.constructId,
                            Properties =
                            {
                                { "questId", questGuid },
                                { "questTaskId", dropGuid }
                            }
                        },
                        new DeliverItemsUnrestrictedTaskDefinition(
                            dropContainer,
                            questItems
                        )
                    )
                }
            )
        );
    }

    private QuestLootCalculationResult CalculateLootPrice(
        Dictionary<string, RecipeOutputData> priceMap,
        ItemBagData.ItemAndQuantity[] entries
    )
    {
        double totalPrice = 0;
        var questItems = new List<QuestElementQuantityRef>();

        foreach (var entry in entries)
        {
            questItems.Add(new QuestElementQuantityRef(
                _bank.IdFor(entry.ItemName),
                entry.ItemName,
                -entry.Quantity.GetRawQuantity()
            ));

            if (priceMap.TryGetValue(entry.ItemName, out var recipeValue))
            {
                totalPrice += entry.Quantity.GetRawQuantity() * recipeValue.GetUnitPrice();
            }
        }

        return new QuestLootCalculationResult
        {
            TotalPrice = totalPrice,
            QuestItems = questItems
        };
    }
}