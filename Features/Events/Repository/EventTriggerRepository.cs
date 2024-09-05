using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Events.Data;
using Mod.DynamicEncounters.Features.Events.Interfaces;

namespace Mod.DynamicEncounters.Features.Events.Repository;

public class EventTriggerRepository(IServiceProvider provider) : IEventTriggerRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();
    
    public async Task<IEnumerable<EventTriggerItem>> FindPendingByEventNameAndPlayerIdAsync(string eventName, ulong? playerId)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            """
            SELECT ET.* FROM public.mod_event_trigger AS ET 
            LEFT JOIN public.mod_event_trigger_tracker AS TT ON (TT.event_trigger_id = ET.id)
            WHERE ((@playerId = 0 AND TT.player_id IS NULL) OR TT.player_id = @playerId) AND ET.event_name = @eventName
                AND TT.id IS NULL
            """,
            new
            {
                eventName,
                playerId = (long)(playerId ?? 0)
            }
        )).ToList();

        return result.Select(MapToItem);
    }

    public async Task AddTriggerTrackingAsync(ulong playerId, Guid eventTriggerId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_event_trigger_tracker (player_id, event_trigger_id) VALUES (@playerId, @eventTriggerId)
            """,
            new
            {
                playerId = (long)playerId,
                eventTriggerId
            }
        );
    }

    private EventTriggerItem MapToItem(DbRow row)
    {
        return new EventTriggerItem(row.event_name, row.on_trigger_script)
        {
            Id = row.id,
            MinTriggerValue = row.min_trigger_value,
            OnTriggerScript = row.on_trigger_script
        };
    }

    public struct DbRow
    {
        public Guid id { get; set; }
        public string event_name { get; set; }
        public double min_trigger_value { get; set; }
        public long player_id { get; set; }
        public string on_trigger_script { get; set; }
    }
}