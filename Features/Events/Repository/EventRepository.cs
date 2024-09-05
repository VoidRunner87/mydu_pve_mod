using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Events.Repository;

public class EventRepository(IServiceProvider provider) : IEventRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task AddAsync(IEvent @event)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_event (id, event_name, event_data, value, player_id)
            VALUES (@id, @event_name, @event_data::jsonb, value, player_id)
            """,
            new
            {
                id = @event.Id,
                event_name = @event.Name,
                event_data = JsonConvert.SerializeObject(@event.Data),
                value = @event.Value,
                player_id = @event.PlayerId
            }
        );
    }

    public async Task<double> GetSumAsync(string eventName, ulong? playerId)
    {
        using var db = _factory.Create();
        db.Open();

        return await db.ExecuteScalarAsync<double>(
            """
            SELECT SUM(value) FROM public.mod_event WHERE ((@playerId = 0 AND player_id IS NULL) OR (player_id = @playerId)) AND eventName = @eventName
            """,
            new
            {
                eventName,
                playerId = (long)(playerId ?? 0)
            }
        );
    }
}