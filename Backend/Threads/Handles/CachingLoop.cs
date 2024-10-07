using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class CachingLoop(IThreadManager threadManager, CancellationToken token) :
    ThreadHandle(ThreadId.Caching, threadManager, token)
{
    private readonly TimeSpan _timeSpan = TimeSpan.FromSeconds(5);
    
    public override async Task Tick()
    {
        try
        {
            var provider = ModBase.ServiceProvider;
            var logger = provider.CreateLogger<CachingLoop>();
            var spatialHashCacheService = provider.GetRequiredService<ISectorSpatialHashCacheService>();

            var sw = new Stopwatch();
            sw.Start();

            var map = await spatialHashCacheService.GetPlayerConstructsSectorMapAsync();

            if (map.Count == 0)
            {
                logger.LogInformation("No Constructs in Sectors of Construct Handles. Time = {Time}ms",
                    sw.ElapsedMilliseconds);
                lock (SectorGridConstructCache.Lock)
                {
                    SectorGridConstructCache.Data = [];
                }

                ReportHeartbeat();
                Thread.Sleep(_timeSpan);

                return;
            }

            lock (SectorGridConstructCache.Lock)
            {
                SectorGridConstructCache.Data = map;
            }

            logger.LogInformation("CacheLoop Took: {Time}ms", sw.ElapsedMilliseconds);

            ReportHeartbeat();

            Thread.Sleep(_timeSpan);
        }
        catch (Exception e)
        {
            var logger = ModBase.ServiceProvider.CreateLogger<CachingLoop>();
            logger.LogError(e, "Failed to Cache Player Map");

            Thread.Sleep(_timeSpan);
        }
    }
}