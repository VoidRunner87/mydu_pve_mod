using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class AreaScanService(IServiceProvider provider) : IAreaScanService
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<IEnumerable<ScanContact>> ScanForPlayerContacts(
        ulong constructId, 
        Vec3 position,
        double radius,
        int limit
    )
    {
        using var db = _factory.Create();
        db.Open();

        var rows = (await db.QueryAsync<DbRow>(
            $"""
             SELECT 
                 C.id, 
                 C.name, 
                 ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) as distance,
                 C.position_x,
                 C.position_y,
                 C.position_z
             FROM public.construct C
             INNER JOIN public.ownership O ON O.id = C.owner_entity_id
             LEFT JOIN mod_npc_construct_handle CH ON (CH.construct_id = C.id)
             WHERE CH.id IS NULL
                 AND ST_DWithin(C.position, ST_MakePoint({VectorToSql(position)}), {radius})
                 AND ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) <= {radius}
                 AND C.deleted_at IS NULL
                 AND (C.json_properties->>'isUntargetable' = 'false' OR C.json_properties->>'isUntargetable' IS NULL)
                 AND (C.json_properties->>'kind' IN ('4', '5'))
                 AND (C.owner_entity_id IS NOT NULL)
                 AND (O.player_id IS NULL OR O.player_id != 4)
                 AND C.id != @constructId
             ORDER BY distance ASC
             LIMIT {limit}
             """,
            new
            {
                constructId = (long)constructId
            }
        )).ToList();

        return rows.Select(MapToModel);
    }

    public async Task<IEnumerable<ScanContact>> ScanForNpcConstructs(Vec3 position, double radius, int limit = 5)
    {
        using var db = _factory.Create();
        db.Open();

        var rows = (await db.QueryAsync<DbRow>(
            $"""
             SELECT 
                 C.id, 
                 FORMAT('[%s] %s', C.id, C.name) as name, 
                 ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) as distance,
                 C.position_x,
                 C.position_y,
                 C.position_z
             FROM public.construct C
             INNER JOIN public.ownership O ON O.id = C.owner_entity_id
             INNER JOIN mod_npc_construct_handle CH ON (CH.construct_id = C.id)
             WHERE ST_DWithin(C.position, ST_MakePoint({VectorToSql(position)}), {radius})
                 AND ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) <= {radius}
                 AND C.deleted_at IS NULL
                 AND CH.deleted_at IS NULL
                 AND (C.json_properties->>'isUntargetable' = 'false' OR C.json_properties->>'isUntargetable' IS NULL)
                 AND (C.json_properties->>'kind' IN ('4', '5'))
                 AND (C.owner_entity_id IS NOT NULL)
                 AND (O.player_id = 4)
             ORDER BY distance ASC
             LIMIT {limit}
             """
        )).ToList();

        return rows.Select(MapToModel);
    }

    public async Task<IEnumerable<ScanContact>> ScanForAbandonedConstructs(Vec3 position, double radius, int limit = 10)
    {
        using var db = _factory.Create();
        db.Open();

        var rows = (await db.QueryAsync<DbRow>(
            $"""
             SELECT 
                 C.id, 
                 C.name, 
                 ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) as distance 
             FROM public.construct C
             WHERE ST_DWithin(C.position, ST_MakePoint({VectorToSql(position)}), {radius})
                 AND ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) <= {radius}
                 AND C.deleted_at IS NULL
                 AND (C.json_properties->>'isUntargetable' = 'false' OR C.json_properties->>'isUntargetable' IS NULL)
                 AND (C.json_properties->>'kind' IN ('4', '5'))
                 AND (C.owner_entity_id IS NULL)
             ORDER BY distance ASC
             LIMIT {limit}
             """
        )).ToList();

        return rows.Select(MapToModel);
    }

    public async Task<IEnumerable<ScanContact>> ScanForAsteroids(Vec3 position, double radius)
    {
        using var db = _factory.Create();
        db.Open();

        var rows = (await db.QueryAsync<DbRow>(
            $"""
              SELECT 
              	 C.id, 
              	 C.name,
              	 ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) as distance 
               FROM public.construct C
               WHERE ST_DWithin(C.position, ST_MakePoint({VectorToSql(position)}), {radius})
              	 AND ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) <= {radius}
              	 AND C.deleted_at IS NULL
              	 AND (C.json_properties->>'kind' = '2')
               ORDER BY distance
              """
        )).ToList();

        return rows.Select(MapToModel);
    }
    
    public async Task<IEnumerable<ScanContact>> ScanForPlanetaryBodies(Vec3 position, double radius)
    {
        using var db = _factory.Create();
        db.Open();

        var rows = (await db.QueryAsync<DbRow>(
            $"""
             SELECT 
             	 C.id, 
             	 C.name,
             	 ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) as distance 
              FROM public.construct C
              WHERE ST_DWithin(C.position, ST_MakePoint({VectorToSql(position)}), {radius})
             	 AND ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) <= {radius}
             	 AND C.deleted_at IS NULL
             	 AND (C.json_properties->>'kind' = '1')
              ORDER BY distance
             """
        )).ToList();

        return rows.Select(MapToModel);
    }

    private static ScanContact MapToModel(DbRow row)
    {
        return new ScanContact(
            row.name,
            (ulong)row.id,
            row.distance,
            new Vec3
            {
                x = row.position_x,
                y = row.position_y,
                z = row.position_z
            }
        );
    }

    private string VectorToSql(Vec3 v)
    {
        return $"{v.x}, {v.y}, {v.z}";
    }

    private struct DbRow
    {
        public long id { get; set; }
        public string name { get; set; }
        public double distance { get; set; }
        public double position_x { get; set; }
        public double position_y { get; set; }
        public double position_z { get; set; }
    }
}