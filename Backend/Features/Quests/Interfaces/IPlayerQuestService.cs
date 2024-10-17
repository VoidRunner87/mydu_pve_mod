using System;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Quests.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public interface IPlayerQuestService
{
    Task<PlayerAcceptQuestOutcome> AcceptQuestAsync(PlayerId playerId, ProceduralQuestItem proceduralQuestItem);
    Task<PlayerAbandonQuestOutcome> AbandonQuestAsync(PlayerId playerId, Guid questId);
}