using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Events.Data;

namespace Mod.DynamicEncounters.Features.Events.Interfaces;

public interface IEventTriggerRepository
{
    Task<IEnumerable<EventTriggerItem>> FindPendingByEventNameAndPlayerIdAsync(string eventName, ulong? playerId);

    Task AddTriggerTrackingAsync(Guid eventTriggerId);
}