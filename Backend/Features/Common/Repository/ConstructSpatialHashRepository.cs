using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Common.Vector;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Services;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Repository;

public class ConstructSpatialHashRepository(IServiceProvider serviceProvider) : IConstructSpatialHashRepository
{
    private readonly IPostgresConnectionFactory _factory =
        serviceProvider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<IEnumerable<ulong>> FindPlayerLiveConstructsOnSector(Vec3 sector)
    {
        using var db = _factory.Create();
        db.Open();
        
        var result = (await db.QueryAsync<ulong>(
            $"""
            SELECT C.id FROM public.construct C
            LEFT JOIN public.ownership O ON (C.owner_entity_id = O.id)
            WHERE C.sector_x = @x AND C.sector_y = @y AND C.sector_z = @z AND
                  C.deleted_at IS NULL AND
                  (C.json_properties->>'isUntargetable' = 'false' OR C.json_properties->>'isUntargetable' IS NULL) AND
                  C.owner_entity_id IS NOT NULL AND (O.player_id NOT IN({StaticPlayerId.Aphelia}, {StaticPlayerId.Unknown}) OR (O.player_id IS NULL AND O.organization_id IS NOT NULL))
            """,
            new
            {
                sector.x,
                sector.y,
                sector.z,
            }
        )).ToList();

        return result;
    }

    public async Task<IEnumerable<ConstructSectorRow>> FindPlayerLiveConstructsOnSectorInstances()
    {
        using var db = _factory.Create();
        db.Open();

        var sectors = (await db.QueryAsync<VectorRow>(
            """
            SELECT DISTINCT C.sector_x as x, C.sector_y as y, C.sector_z as z 
            FROM public.mod_npc_construct_handle CH
            INNER JOIN public.construct C ON (C.id = CH.construct_id)
            WHERE C.deleted_at IS NULL AND CH.deleted_at IS NULL 
            """
        )).ToList();

        if (sectors.Count == 0)
        {
            return new List<ConstructSectorRow>();
        }

        var sectorQueries = sectors.Select(v => $"(C.sector_x = {(long)v.x} AND C.sector_y = {(long)v.y} AND C.sector_z = {(long)v.z})");
        var sectorQuery = string.Join(" OR ", sectorQueries);
        
        var result = (await db.QueryAsync<ConstructSectorRow>(
            $"""
             SELECT C.id, C.name, C.sector_x, C.sector_y, C.sector_z FROM public.construct C
             LEFT JOIN public.ownership O ON (C.owner_entity_id = O.id)
             WHERE C.deleted_at IS NULL AND
             (C.json_properties->>'isUntargetable' = 'false' OR C.json_properties->>'isUntargetable' IS NULL) AND
             C.owner_entity_id IS NOT NULL AND
             (
             	O.player_id NOT IN({StaticPlayerId.Aphelia}, {StaticPlayerId.Unknown}) OR 
             	(O.player_id IS NULL AND O.organization_id IS NOT NULL)
             ) AND (
                 {sectorQuery}
             )
             """
        )).ToList();

        return result;
    }

    public struct VectorRow
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }
    
    public struct ConstructSectorRow
    {
        public long id { get; set; }
        public string name { get; set; }
        public long sector_x { get; set; }
        public long sector_y { get; set; }
        public long sector_z { get; set; }

        public ulong ConstructId() => (ulong)id;
        public LongVector3 GetLongVector() => new(new Vec3{x = sector_x, y = sector_y, z = sector_z}.GridSnap(SectorPoolManager.SectorGridSnap));
    }
}