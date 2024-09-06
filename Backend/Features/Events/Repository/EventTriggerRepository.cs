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
    
    public async Task<IEnumerable<EventTriggerItem>> FindByEventNameAsync(string eventName)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            """
            SELECT * FROM public.mod_event_trigger 
            WHERE event_name = @eventName
            """,
            new
            {
                eventName
            }
        )).ToList();

        return result.Select(MapToItem);
    }
    
    public async Task<HashSet<Guid>> GetTrackedEventTriggers(IEnumerable<Guid> eventTriggerIds, ulong playerId)
    {
        using var db = _factory.Create();
        db.Open();

        var quidsInQuery = string.Join(",", eventTriggerIds.Select(x => $"'{x}'")); 
        
        var result = (await db.QueryAsync<DbGroupByCountById>(
            $"""
            SELECT COUNT(0) as count, ET.id FROM public.mod_event_trigger_tracker AS TT
            LEFT JOIN public.mod_event_trigger AS ET ON (TT.event_trigger_id = ET.id)
            WHERE TT.player_id = @playerId AND ET.id IN ({quidsInQuery})
            GROUP BY ET.id
            """,
            new
            {
                playerId = (long)playerId
            }
        )).ToList();

        return result.Select(x => x.id).ToHashSet();
    }

    public async Task AddTriggerTrackingAsync(ulong playerId, Guid eventTriggerId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_event_trigger_tracker (id, player_id, event_trigger_id) VALUES (@id, @playerId, @eventTriggerId)
            """,
            new
            {
                id = Guid.NewGuid(),
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

    public struct DbGroupByCountById
    {
        public long count { get; set; }
        public Guid id { get; set; }
    }
}