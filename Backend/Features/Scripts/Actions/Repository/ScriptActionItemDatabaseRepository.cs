using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Repository;

public class ScriptActionItemDatabaseRepository(IServiceProvider provider) : IScriptActionItemRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task AddAsync(ScriptActionItem item)
    {
        using var db = _factory.Create();
        db.Open();

        if (item.Id == Guid.Empty)
        {
            item.Id = Guid.NewGuid();
        }

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_script (id, name, content) 
            VALUES(@id, @name, @content::jsonb)
            """,
            new
            {
                id = item.Id,
                name = item.Name,
                content = JsonConvert.SerializeObject(item)
            }
        );
    }

    public Task SetAsync(IEnumerable<ScriptActionItem> items)
    {
        throw new NotSupportedException();
    }

    public Task AddRangeAsync(IEnumerable<ScriptActionItem> items)
    {
        throw new NotImplementedException("TODO LATER");
    }

    public async Task<ScriptActionItem?> FindAsync(object key)
    {
        using var db = _factory.Create();
        db.Open();

        var rows = (await db.QueryAsync<DbRow>(
                """SELECT * FROM public.mod_script WHERE name = @key""",
                new { key })
            ).ToList();

        if (rows.Count == 0)
        {
            return null;
        }

        return MapToModel(rows[0]);
    }

    public async Task<IEnumerable<ScriptActionItem>> GetAllAsync()
    {
        using var db = _factory.Create();
        db.Open();

        var rows = (await db.QueryAsync<DbRow>("""
                                               SELECT * FROM public.mod_script
                                               """)).ToList();

        return rows.Select(MapToModel);
    }

    public async Task<long> GetCountAsync()
    {
        using var db = _factory.Create();
        db.Open();

        return await db.ExecuteScalarAsync<int>("""
                                                SELECT COUNT(0) FROM public.mod_script
                                                """);
    }

    public async Task DeleteAsync(object key)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync("DELETE FROM public.mod_script WHERE name = @key", new { key });
    }

    public Task Clear()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ActionExistAsync(string actionName)
    {
        using var db = _factory.Create();
        db.Open();

        return await db.ExecuteScalarAsync<long>(
            "SELECT COUNT(0) FROM public.mod_script WHERE name = @actionName",
            new { actionName }
        ) > 0;
    }

    private ScriptActionItem MapToModel(DbRow row)
    {
        var model = JsonConvert.DeserializeObject<ScriptActionItem>(row.content);
        model.Id = row.id;
        
        return model;
    }

    private struct DbRow
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string content { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}