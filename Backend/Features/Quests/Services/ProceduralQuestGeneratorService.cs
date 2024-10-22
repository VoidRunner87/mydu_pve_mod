using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Faction.Data;
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
        var timeFactor = TimeUtility.GetTimeSnapped(DateTimeOffset.UtcNow, MissionProceduralGenerationConfig.TimeFactor);
        var random = new Random(seed + (int)timeFactor);
        var result = new List<ProceduralQuestItem>();

        for (var i = 0; i < quantity; i++)
        {
            var questSeed = random.Next();
            var questType = random.PickOneAtRandom(QuestTypes.All());

            switch (questType)
            {
                case QuestTypes.Transport:
                    var transportGen = provider.GetRequiredService<IProceduralTransportMissionGeneratorService>();
                    var transportOutcome = await transportGen.GenerateAsync(playerId, factionId, territoryId, questSeed);
                    if (transportOutcome.Success)
                    {
                        result.Add(transportOutcome.QuestItem);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to Generate Quest: {Message}", transportOutcome.Message);
                    }

                    break;
                case QuestTypes.ReverseTransport:
                    var reverseTransportGen = provider.GetRequiredService<IProceduralReverseTransportMissionGeneratorService>();
                    var reverseTransportOutcome = await reverseTransportGen.GenerateAsync(playerId, factionId, territoryId, questSeed);
                    if (reverseTransportOutcome.Success)
                    {
                        result.Add(reverseTransportOutcome.QuestItem);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to Generate Quest: {Message}", reverseTransportOutcome.Message);
                    }

                    break;
            }
        }

        // remove duplicates of the same mission
        result = result.DistinctBy(x => x.Id).ToList();

        return GenerateQuestListOutcome.WithAvailableQuests(result);
    }
}


