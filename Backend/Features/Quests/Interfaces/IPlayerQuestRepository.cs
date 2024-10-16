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
    Task<IEnumerable<PlayerQuestItem>> GetAll(PlayerId playerId);
    Task DeleteAsync(PlayerId playerId, Guid id);
}