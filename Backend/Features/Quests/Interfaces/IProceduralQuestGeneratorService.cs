using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Quests.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public interface IProceduralQuestGeneratorService
{
    Task<GenerateQuestListOutcome> Generate(
        PlayerId playerId,
        FactionId factionId,
        TerritoryId territoryId,
        int seed,
        int quantity = 40);
}