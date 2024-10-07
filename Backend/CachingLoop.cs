using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class CachingLoop(TimeSpan timerSpan) : ModBase
{
    public override async Task Loop()
    {
        while (true)
        {
            try
            {
                var provider = ServiceProvider;
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
                    
                    await Task.Delay(timerSpan);

                    return;
                }

                lock (SectorGridConstructCache.Lock)
                {
                    SectorGridConstructCache.Data = map;
                }

                logger.LogInformation("CacheLoop Took: {Time}ms", sw.ElapsedMilliseconds);
                
                RecordHeartBeat();
                
                await Task.Delay(timerSpan);
            }
            catch (Exception e)
            {
                var logger = ServiceProvider.CreateLogger<CachingLoop>();
                logger.LogError(e, "Failed to Cache Player Map");
                
                await Task.Delay(timerSpan);
            }
        }
    }
}