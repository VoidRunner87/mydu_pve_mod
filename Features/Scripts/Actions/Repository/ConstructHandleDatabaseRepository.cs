using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Newtonsoft.Json;
using NQ;
using Serilog;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Repository;

public class ConstructHandleDatabaseRepository(IServiceProvider provider) : IConstructHandleRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();
    
    private const string NpcConstructHandleTable = "mod_npc_construct_handle";
    
    public async Task AddAsync(ConstructHandleItem item)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            $"""
             INSERT INTO {NpcConstructHandleTable} (id, construct_id, sector_x, sector_y, sector_z, construct_def_id)
             VALUES (@id, @construct_id, @sector_x, @sector_y, @sector_z, @construct_def_id)
             """,
            new
            {
                id = Guid.NewGuid(),
                construct_id = (long)item.ConstructId,
                sector_x = item.Sector.x,
                sector_y = item.Sector.y,
                sector_z = item.Sector.z,
                construct_def_id = item.ConstructDefinitionId
            }
        );
    }

    public Task SetAsync(IEnumerable<ConstructHandleItem> items)
    {
        throw new NotSupportedException();
    }

    public Task AddRangeAsync(IEnumerable<ConstructHandleItem> items)
    {
        throw new NotSupportedException();
    }

    public async Task<ConstructHandleItem?> FindAsync(object key)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
           """
           SELECT * FROM public.mod_npc_construct_handle WHERE id = @key
           """,
           new {key}
        )).ToList();

        if (result.Count == 0)
        {
            return null;
        }

        return MapToModel(result[0]);
    }

    public async Task<IEnumerable<ConstructHandleItem>> GetAllAsync()
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            """
            SELECT 
                CH.*,
                CD.name as def_name,
                CD.content as def_content
            FROM public.mod_npc_construct_handle CH
            INNER JOIN public.mod_construct_def CD ON (CD.id = CH.construct_def_id)
            """
        )).ToList();

        return result.Select(MapToModel);
    }

    public async Task<long> GetCountAsync()
    {
        using var db = _factory.Create();
        db.Open();

        return await db.ExecuteScalarAsync<long>(
            """
            SELECT COUNT(0) FROM public.mod_npc_construct_handle
            """
        );
    }

    public async Task DeleteAsync(object key)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            DELETE FROM public.mod_npc_construct_handle WHERE id = @key
            """,
            new { key }
        );
    }

    public async Task<IEnumerable<ConstructHandleItem>> FindExpiredAsync(int minutes = 30)
    {
        minutes = Math.Clamp(minutes, 5, 120);
        
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            $"""
            SELECT * FROM public.mod_npc_construct_handle 
            WHERE last_controlled_at + INTERVAL '{minutes} minutes' < NOW();
            """
        )).ToList();

        return result.Select(MapToModel);
    }

    public async Task UpdateLastControlledDateAsync(HashSet<ulong> constructIds)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            UPDATE public.mod_npc_construct_handle
            SET last_controlled_at = NOW()
            WHERE construct_id IN (@ids)
            """,
            new
            {
                ids = constructIds
            }
        );
    }

    private ConstructHandleItem MapToModel(DbRow row)
    {
        ConstructDefinitionItem? constructDefinition = null;

        if (!string.IsNullOrEmpty(row.def_content))
        {
            constructDefinition = JsonConvert.DeserializeObject<ConstructDefinitionItem>(row.def_content);
        }

        return new ConstructHandleItem
        {
            Id = row.id,
            Sector = new Vec3
            {
                x = row.sector_x,
                y = row.sector_y,
                z = row.sector_z,
            },
            ConstructId = (ulong)row.construct_id,
            ConstructDefinitionId = row.construct_def_id,
            ConstructDefinitionItem = constructDefinition
        };
    }

    private struct DbRow
    {
        public Guid id { get; set; }
        public long construct_id { get; set; }
        public Guid construct_def_id { get; set; }
        public long sector_x { get; set; }
        public long sector_y { get; set; }
        public long sector_z { get; set; }
        public DateTime last_controlled_at { get; set; }
        public string? def_content { get; set; }
    }
}