using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Commands.Data;
using Mod.DynamicEncounters.Features.Commands.Interfaces;

namespace Mod.DynamicEncounters.Features.Commands.Repository;

public class PendingCommandRepository(IServiceProvider provider) : IPendingCommandRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<IEnumerable<CommandItem>> QueryAsync(DateTime afterDateTime)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            """
            SELECT id, sender_id, message, date FROM public.chat_message 
            WHERE date >= @date AND message ~ '^@' AND sender_id != @id
            ORDER BY date
            """,
            new { date = afterDateTime, id = (long)ModBase.Bot.PlayerId.id }
        )).ToList();

        return result.Select(MapToModel);
    }

    private CommandItem MapToModel(DbRow row)
    {
        return new CommandItem
        {
            Id = row.id,
            Date = row.date,
            PlayerId = (ulong)row.sender_id,
            Message = row.message
        };
    }

    private struct DbRow
    {
        public long id { get; set; }
        public string message { get; set; }
        public long sender_id { get; set; }
        public DateTime date { get; set; }
    }
}