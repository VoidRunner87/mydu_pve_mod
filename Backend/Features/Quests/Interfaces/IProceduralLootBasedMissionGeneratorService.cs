using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Quests.Services;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public interface IProceduralLootBasedMissionGeneratorService
{
    Task<ProceduralQuestOutcome> GenerateAsync(
        PlayerId playerId,
        FactionId factionId,
        TerritoryId territoryId,
        int seed
    );
}