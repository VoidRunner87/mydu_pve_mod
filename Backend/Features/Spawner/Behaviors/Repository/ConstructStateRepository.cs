using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Repository;

public class ConstructStateRepository(IServiceProvider provider) : IConstructStateRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<ConstructStateItem?> Find(ulong constructId, string type)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            """
            SELECT * FROM mod_construct_state WHERE type = @type AND construct_id = @construct_id
            LIMIT 1
            """,
            new
            {
                type,
                construct_id = (long)constructId
            }
        )).ToList();

        if (result.Count == 0)
        {
            return null;
        }

        return MapToModel(result.First());
    }

    public async Task Add(ConstructStateItem item)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO mod_construct_state (construct_id, type, properties) VALUES (@construct_id, @type, @properties::jsonb)
            """,
            new
            {
                type = item.Type,
                construct_id = (long)item.ConstructId,
                properties = item.Properties?.ToString() ?? "{}"
            }
        );
    }

    public async Task Update(ConstructStateItem item)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            UPDATE mod_construct_state SET
                properties = @properties::jsonb,
                updated_at = NOW()
            WHERE type = @type AND construct_id = @construct_id
            """,
            new
            {
                type = item.Type,
                construct_id = (long)item.ConstructId,
                properties = item.Properties.ToString()
            }
        );
    }

    private ConstructStateItem MapToModel(DbRow row)
    {
        return new ConstructStateItem
        {
            Id = row.id,
            Type = row.type,
            ConstructId = row.construct_id,
            Properties = JToken.Parse(row.properties),
            CreatedAt = row.created_at,
            UpdatedAt = row.updated_at
        };
    }

    private class DbRow
    {
        public Guid id { get; set; }
        public ulong construct_id { get; set; }
        public string type { get; set; }
        public string properties { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}