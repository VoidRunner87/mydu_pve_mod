using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Vector;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Services;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class SectorSpatialHashCacheServiceService(IServiceProvider provider) : ISectorSpatialHashCacheService
{
    private readonly IConstructSpatialHashRepository _repository =
        provider.GetRequiredService<IConstructSpatialHashRepository>();

    private readonly ILogger<SectorSpatialHashCacheServiceService> _logger =
        provider.CreateLogger<SectorSpatialHashCacheServiceService>();

    public async Task<ConcurrentDictionary<LongVector3, ConcurrentBag<ulong>>> GetPlayerConstructsSectorMapAsync()
    {
        var sw = new Stopwatch();
        sw.Start();
        
        const long gridSnap = (long)SectorPoolManager.SectorGridSnap;
        
        var items = await _repository.FindPlayerLiveConstructsOnSectorInstances();
        
        _logger.LogDebug("Query GetPlayerConstructsSectorMapAsync Took: {Time}ms", sw.ElapsedMilliseconds);

        var map = new ConcurrentDictionary<LongVector3, ConcurrentBag<ulong>>();

        var offsets = SectorGridConstructCache.GetOffsets(gridSnap).ToList();
        
        foreach (var item in items)
        {
            foreach (var offset in offsets)
            {
                map.AddOrUpdate(
                    item.GetLongVector() + offset,
                    [item.ConstructId()],
                    (_, bag) =>
                    {
                        bag = new ConcurrentBag<ulong>(bag.ToHashSet()) { item.ConstructId() };

                        return bag;
                    });
            }
        }

        return map;
    }
}