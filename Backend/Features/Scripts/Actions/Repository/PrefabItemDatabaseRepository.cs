using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Repository;

public class PrefabItemDatabaseRepository(IServiceProvider provider) : IPrefabItemRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task AddAsync(PrefabItem item)
    {
        using var db = _factory.Create();
        db.Open();

        if (item.Id == Guid.Empty)
        {
            item.Id = Guid.NewGuid();
        }

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_construct_def (id, name, content) 
            VALUES(@id, @name, @content::jsonb)
            """,
            new
            {
                id =  item.Id,
                name = item.Name,
                content = JsonConvert.SerializeObject(item)
            }
        );
    }

    public Task SetAsync(IEnumerable<PrefabItem> items)
    {
        throw new NotSupportedException();
    }

    public Task UpdateAsync(PrefabItem item)
    {
        throw new NotImplementedException();
    }

    public Task AddRangeAsync(IEnumerable<PrefabItem> items)
    {
        throw new NotImplementedException("TODO LATER");
    }

    public async Task<PrefabItem?> FindAsync(string key)
    {
        using var db = _factory.Create();
        db.Open();

        var rows = (await db.QueryAsync<DbRow>(
                """SELECT * FROM public.mod_construct_def WHERE name = @key""",
                new { key })
            ).ToList();

        return MapToModel(rows[0]);
    }

    public async Task<IEnumerable<PrefabItem>> GetAllAsync()
    {
        using var db = _factory.Create();
        db.Open();

        var rows = (await db.QueryAsync<DbRow>("""
                                               SELECT * FROM public.mod_construct_def
                                               """)).ToList();

        return rows.Select(MapToModel);
    }

    public async Task<long> GetCountAsync()
    {
        using var db = _factory.Create();
        db.Open();

        return await db.ExecuteScalarAsync<int>("""
                                                SELECT COUNT(0) FROM public.mod_construct_def
                                                """);
    }
    
    public async Task DeleteAsync(Guid key)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync("DELETE FROM public.mod_construct_def WHERE id = @key", new { key });
    }

    public async Task DeleteAsync(string key)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync("DELETE FROM public.mod_construct_def WHERE name = @key", new { key });
    }

    public Task Clear()
    {
        throw new NotImplementedException();
    }

    private PrefabItem MapToModel(DbRow row)
    {
        var result = JsonConvert.DeserializeObject<PrefabItem>(row.content);
        result.Id = row.id;
        result.Name = row.name;
        
        return result;
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