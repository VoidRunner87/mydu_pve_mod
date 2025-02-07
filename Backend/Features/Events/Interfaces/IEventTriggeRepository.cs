using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Events.Data;

namespace Mod.DynamicEncounters.Features.Events.Interfaces;

public interface IEventTriggerRepository
{
    Task<IEnumerable<EventTriggerItem>> FindByEventNameAsync(string eventName);
    Task<HashSet<Guid>> GetTrackedEventTriggers(IEnumerable<Guid> eventTriggerIds, ulong playerId);

    Task AddTriggerTrackingAsync(ulong playerId, Guid eventTriggerId);
    Task<long> GetCountOfEventsByPlayerId(ulong playerId, string eventName);
}