using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Quests.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public interface IPlayerQuestRepository
{
    Task AddAsync(PlayerQuestItem item);
    Task UpdateAsync(PlayerQuestItem item);
    Task<PlayerQuestItem?> GetAsync(QuestId id);
    Task<IEnumerable<PlayerQuestItem>> GetAllAsync(PlayerId playerId);
    Task<IEnumerable<PlayerQuestItem>> GetAllByStatusAsync(PlayerId playerId, string status);
    Task DeleteAsync(PlayerId playerId, Guid id);
    Task CompleteTaskAsync(QuestTaskId questTaskId);
    Task SetStatusAsync(QuestId questId, string status);
    Task<bool> AreAllTasksCompleted(QuestId questId);
}