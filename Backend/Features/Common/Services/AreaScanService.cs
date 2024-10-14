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

public class AreaScanService(IServiceProvider provider) : INpcRadarService
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<IEnumerable<NpcRadarContact>> ScanForPlayerContacts(
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
                 ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) as distance 
             FROM public.construct C
             LEFT JOIN mod_npc_construct_handle CH ON (CH.construct_id = C.id)
             WHERE CH.id IS NULL
                 AND ST_DWithin(C.position, ST_MakePoint({VectorToSql(position)}), {radius})
                 AND ST_3DDistance(C.position, ST_MakePoint({VectorToSql(position)})) <= {radius}
                 AND C.deleted_at IS NULL
                 AND (C.json_properties->>'isUntargetable' = 'false' OR C.json_properties->>'isUntargetable' IS NULL)
                 AND (C.json_properties->>'kind' IN ('4', '5'))
                 AND (C.owner_entity_id IS NOT NULL)
             ORDER BY distance ASC
             LIMIT {limit}
             """
        )).ToList();

        return rows.Select(MapToModel);
    }

    

    private static NpcRadarContact MapToModel(DbRow row)
    {
        return new NpcRadarContact(
            row.name,
            (ulong)row.id
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
    }
}