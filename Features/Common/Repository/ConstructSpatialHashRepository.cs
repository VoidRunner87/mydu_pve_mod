using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Repository;

public class ConstructSpatialHashRepository(IServiceProvider serviceProvider) : IConstructSpatialHashRepository
{
    private readonly IPostgresConnectionFactory _factory = serviceProvider.GetRequiredService<IPostgresConnectionFactory>();
    
    public async Task<IEnumerable<ulong>> FindPlayerLiveConstructsOnSector(Vec3 sector)
    {
        using var db = _factory.Create();
        db.Open();

        
        var result = (await db.QueryAsync<ulong>(
            $"""
            SELECT C.id FROM public.construct C
            LEFT JOIN public.ownership O ON (C.owner_entity_id = O.id)
            WHERE C.sector_x = @x AND C.sector_y = @y AND C.sector_z = @z AND
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
}