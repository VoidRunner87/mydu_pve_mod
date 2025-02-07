using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Repository;

public class ConstructInMemoryBehaviorContextRepository : IConstructInMemoryBehaviorContextRepository
{
    private readonly ConcurrentDictionary<ulong, ShortLivedBehaviorContextEntry> _entries = new();
    
    public bool TryGetValue(ulong constructId, out BehaviorContext? context)
    {
        if (_entries.TryGetValue(constructId, out var entry))
        {
            context = entry.BehaviorContext;
            return true;
        }

        context = default;
        return false;
    }

    public void Set(ulong constructId, BehaviorContext context)
    {
        if (!_entries.TryAdd(
                constructId,
                new ShortLivedBehaviorContextEntry(
                    context,
                    DateTime.UtcNow + TimeSpan.FromMinutes(10)
                )
            ))
        {
            _entries[constructId] = new ShortLivedBehaviorContextEntry(
                context,
                DateTime.UtcNow + TimeSpan.FromMinutes(10)
            );
        }
    }

    public void Cleanup()
    {
        var expiredEntries = _entries.Where(e => DateTime.UtcNow > e.Value.ExpiresAt);

        const int maxloop = 10;
        var i = 0;
        foreach (var kvp in expiredEntries)
        {
            // let it do it again on the next cycle
            if (i > maxloop) break;
            
            _entries.Remove(kvp.Key, out _);
            i++;
        }
    }
}