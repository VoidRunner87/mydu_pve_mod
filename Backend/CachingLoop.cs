using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class CachingLoop(TimeSpan timerSpan) : ModBase
{
    public override Task Start()
    {
        var taskCompletionSource = new TaskCompletionSource();
        var logger = ServiceProvider.CreateLogger<CachingLoop>();
        
        var timer = new Timer(timerSpan);
        timer.Elapsed += async (_, _) =>
        {
            try
            {
                await OnTimer();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to run Cache Loop Timer");
            }
            
            RecordHeartBeat();
        };
        timer.Start();

        return taskCompletionSource.Task;
    }

    private async Task OnTimer()
    {
        var provider = ServiceProvider;
        var logger = provider.CreateLogger<CachingLoop>();
        var spatialHashCacheService = provider.GetRequiredService<ISectorSpatialHashCacheService>();

        var sw = new Stopwatch();
        sw.Start();

        try
        {
            var map = await spatialHashCacheService.GetPlayerConstructsSectorMapAsync();

            if (map.Count == 0)
            {
                logger.LogInformation("No Constructs in Sectors of Construct Handles. Time = {Time}ms", sw.ElapsedMilliseconds);
                lock (SectorGridConstructCache.Lock)
                {
                    SectorGridConstructCache.Data = [];
                }
                return;
            }
        
            lock (SectorGridConstructCache.Lock)
            {
                SectorGridConstructCache.Data = map;
            }

            // foreach (var kvp in map)
            // {
            //     foreach (var constructId in kvp.Value)
            //     {
            //         logger.LogInformation("Sector: {Sector} | Construct: {Construct}", kvp.Key, constructId);
            //     }
            // }

            logger.LogInformation("CacheLoop Took: {Time}ms", sw.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to Cache Player Map");
        }
    }
}