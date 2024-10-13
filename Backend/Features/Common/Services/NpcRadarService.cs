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

public class NpcRadarService(IServiceProvider provider) : INpcRadarService
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<IEnumerable<NpcRadarContact>> ScanForContacts(
        ulong constructId,
        Vec3 position,
        double radius
    )
    {
        using var db = _factory.Create();
        db.Open();

        var rows = (await db.QueryAsync<DbRow>(
            $"""
             SELECT 
                 id, 
                 name, 
                 ST_3DDistance(position, ST_MakePoint({VectorToSql(position)})) as distance 
             FROM public.construct
             WHERE ST_DWithin(position, ST_MakePoint({VectorToSql(position)}), {radius})
                 AND ST_3DDistance(position, ST_MakePoint({VectorToSql(position)})) <= {radius}
                 AND id != @constructId
                 AND deleted_at IS NULL
             ORDER BY distance ASC
             """,
            new
            {
                constructId = (long)constructId
            }
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