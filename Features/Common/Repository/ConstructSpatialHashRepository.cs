using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Repository;

public class ConstructSpatialHashRepository(IServiceProvider serviceProvider) : IConstructSpatialHashRepository
{
    private readonly IPostgresConnectionFactory _factory = serviceProvider.GetRequiredService<IPostgresConnectionFactory>();
    
    public async Task<IEnumerable<ulong>> FindConstructsOnSector(Vec3 sector)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<ulong>(
            "SELECT id FROM public.construct WHERE sector_x = @x AND sector_y = @y AND sector_z = @z",
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