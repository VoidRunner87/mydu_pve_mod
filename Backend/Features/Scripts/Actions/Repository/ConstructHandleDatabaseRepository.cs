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
             INSERT INTO {NpcConstructHandleTable} (id, construct_id, faction_id, sector_x, sector_y, sector_z, construct_def_id, original_owner_player_id, original_organization_id, on_cleanup_script, json_properties)
             VALUES (@id, @construct_id, @faction_id, @sector_x, @sector_y, @sector_z, @construct_def_id, @original_owner_player_id, @original_organization_id, @on_cleanup_script, @json_properties::jsonb)
             """,
            new
            {
                id = Guid.NewGuid(),
                construct_id = (long)item.ConstructId,
                faction_id = item.FactionId,
                sector_x = item.Sector.x,
                sector_y = item.Sector.y,
                sector_z = item.Sector.z,
                construct_def_id = item.ConstructDefinitionId,
                original_owner_player_id = (long)item.OriginalOwnerPlayerId,
                original_organization_id = (long)item.OriginalOrganizationId,
                on_cleanup_script = item.OnCleanupScript,
                json_properties = JsonConvert.SerializeObject(item.JsonProperties)
            }
        );
    }

    public Task SetAsync(IEnumerable<ConstructHandleItem> items)
    {
        throw new NotSupportedException();
    }

    public Task UpdateAsync(ConstructHandleItem item)
    {
        throw new NotImplementedException();
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
            SELECT * FROM public.mod_npc_construct_handle 
            WHERE id = @key AND deleted_at IS NULL
            """,
            new { key }
        )).ToList();

        if (result.Count == 0)
        {
            return null;
        }

        return MapToModel(result[0]);
    }

    public async Task<ConstructHandleItem?> FindByConstructIdAsync(ulong constructId)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            """
            SELECT * FROM public.mod_npc_construct_handle 
            WHERE construct_id = @constructId AND deleted_at IS NULL 
            LIMIT 1
            """,
            new { constructId = (long)constructId }
        )).ToList();

        if (result.Count == 0)
        {
            return null;
        }

        return MapToModel(result[0]);
    }

    public async Task<IEnumerable<ConstructHandleItem>> FindActiveHandlesAsync()
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
            INNER JOIN public.construct C ON (C.id = CH.construct_id)
            WHERE NOT (CD.content->'InitialBehaviors' @> '"wreck"') AND
            	CH.deleted_at IS NULL AND 
            	C.deleted_at IS NULL
            """
        )).ToList();

        return result.Select(MapToModel);
    }

    public async Task<IEnumerable<ulong>> FindAllBuggedPoiConstructsAsync()
    {
        await Task.Yield();

        using var db = _factory.Create();
        db.Open();

        var results = await db.QueryAsync<long>(
            """
            SELECT C.id FROM construct C
            LEFT JOIN mod_npc_construct_handle CH ON (C.id = CH.construct_id)
            WHERE (CH.id IS NULL OR CH.deleted_at IS NOT NULL) AND
                C.deleted_at IS NULL AND
                C.owner_entity_id IS NULL AND
                C.json_properties->'serverProperties'->>'isDynamicWreck' = 'true' AND
                C.json_properties->'kind' = '4'
            """
        );

        return results.Select(x => (ulong)x);
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
            WHERE CH.deleted_at IS NULL
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
            SELECT COUNT(0) FROM public.mod_npc_construct_handle WHERE deleted_at IS NULL
            """
        );
    }

    public async Task DeleteAsync(object key)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            UPDATE public.mod_npc_construct_handle 
                SET deleted_at = NOW() 
            WHERE id = @key
            """,
            new { key = (Guid)key }
        );
    }

    public async Task DeleteByConstructId(ulong constructId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            UPDATE public.mod_npc_construct_handle 
                SET deleted_at = NOW() 
            WHERE construct_id = @key
            """,
            new { key = (long)constructId }
        );
    }

    public Task Clear()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<ConstructHandleItem>> FindTagInSectorAsync(Vec3 sector, string tag)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            $"""
             SELECT * FROM public.mod_npc_construct_handle 
                WHERE sector_x = @x AND sector_y = @y AND sector_z = @z AND
                json_properties->'Tags' @> @tag::jsonb AND
                deleted_at IS NULL
             """,
            new
            {
                sector.x,
                sector.y,
                sector.z,
                tag = $"\"{tag}\""
            }
        )).ToList();

        return result.Select(MapToModel);
    }

    public async Task<IEnumerable<ConstructHandleItem>> FindInSectorAsync(Vec3 sector)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            $"""
             SELECT * FROM public.mod_npc_construct_handle WHERE
             sector_x = @x AND sector_y = @y AND sector_z = @z AND
             deleted_at IS NULL
             """,
            new
            {
                sector.x,
                sector.y,
                sector.z,
            }
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
                ids = constructIds.Select(x => (long)x).ToList()
            }
        );
    }

    public async Task RemoveHandleAsync(ulong constructId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            UPDATE public.mod_npc_construct_handle
            SET deleted_at = NOW() 
            WHERE construct_id = @constructId
            """,
            new { constructId = (long)constructId }
        );
    }

    public async Task<Dictionary<ulong, TimeSpan>> GetPoiConstructExpirationTimeSpansAsync()
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<ConstructTimeSpanRow>(
            """
            SELECT CH.construct_id, SI.expires_at - NOW() time_span FROM public.mod_npc_construct_handle CH
            INNER JOIN public.mod_sector_instance SI ON (SI.sector_x = CH.sector_x AND SI.sector_y = CH.sector_y AND SI.sector_z = CH.sector_z)
            INNER JOIN public.construct C ON (C.id = CH.construct_id)
            WHERE CH.json_properties->'Tags' @> '"poi"' AND
            	CH.deleted_at IS NULL AND
            	C.owner_entity_id IS NULL AND
            	C.deleted_at IS NULL
            """)).ToList();

        var distinctResult = result.DistinctBy(x => x.construct_id);
        
        return distinctResult.ToDictionary(
            k => (ulong)k.construct_id,
            v => v.time_span
        );
    }

    private ConstructHandleItem MapToModel(DbRow row)
    {
        PrefabItem? constructDefinition = null;

        if (!string.IsNullOrEmpty(row.def_content))
        {
            constructDefinition = JsonConvert.DeserializeObject<PrefabItem>(row.def_content);
        }

        var properties = new ConstructHandleProperties();
        if (row.json_properties != null)
        {
            properties = JsonConvert.DeserializeObject<ConstructHandleProperties>(row.json_properties);
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
            FactionId = row.faction_id,
            ConstructId = (ulong)row.construct_id,
            ConstructDefinitionId = row.construct_def_id,
            OriginalOwnerPlayerId = row.original_owner_player_id,
            OriginalOrganizationId = row.original_organization_id,
            OnCleanupScript = row.on_cleanup_script,
            JsonProperties = properties,
            ConstructDefinitionItem = constructDefinition
        };
    }

    private struct ConstructTimeSpanRow
    {
        public long construct_id { get; set; }
        public TimeSpan time_span { get; set; }
    }

    private struct DbRow
    {
        public Guid id { get; set; }
        public long faction_id { get; set; }
        public long construct_id { get; set; }
        public Guid construct_def_id { get; set; }
        public double sector_x { get; set; }
        public double sector_y { get; set; }
        public double sector_z { get; set; }
        public DateTime last_controlled_at { get; set; }
        public string? def_content { get; set; }
        public ulong original_owner_player_id { get; set; }
        public ulong original_organization_id { get; set; }
        public string on_cleanup_script { get; set; }
        public string? json_properties { get; set; }
    }
}