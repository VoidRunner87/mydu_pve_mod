using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Vector;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Services;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class SectorSpatialHashCacheServiceService(IServiceProvider provider) : ISectorSpatialHashCacheService
{
    private readonly IConstructSpatialHashRepository _repository =
        provider.GetRequiredService<IConstructSpatialHashRepository>();

    public async Task<ConcurrentDictionary<LongVector3, ConcurrentBag<ulong>>> GetPlayerConstructsSectorMapAsync()
    {
        const long gridSnap = (long)SectorPoolManager.SectorGridSnap;
        
        var items = await _repository.FindPlayerLiveConstructsOnSectorInstances();

        var map = new ConcurrentDictionary<LongVector3, ConcurrentBag<ulong>>();

        var offsets = GetOffsets(gridSnap).ToList();
        
        foreach (var item in items)
        {
            foreach (var offset in offsets)
            {
                if (!map.TryAdd(item.GetLongVector() + offset, [item.ConstructId()]))
                {
                    map[item.GetLongVector()].Add(item.ConstructId());
                }
            }
        }

        return map;
    }
    
    private IEnumerable<LongVector3> GetOffsets(long gridSnap, int radius = 1)
    {
        var offsets = new List<LongVector3>();

        for (long x = -radius; x <= radius; x++)
        {
            for (long y = -radius; y <= radius; y++)
            {
                for (long z = -radius; z <= radius; z++)
                {
                    offsets.Add(new LongVector3(x * gridSnap, y * gridSnap, z * gridSnap));
                }
            }
        }

        return offsets;
    }
}