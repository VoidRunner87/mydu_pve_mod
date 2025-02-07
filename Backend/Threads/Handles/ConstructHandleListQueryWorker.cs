using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class ConstructHandleListQueryWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);
            
                await Tick(cts.Token);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (Exception e)
            {
                ModBase.ServiceProvider.CreateLogger<ConstructHandleListQueryWorker>()
                    .LogError(e, "{Type} Exception: {Message}", GetType().Name, e.Message);
            }
        }
    }
    
    private async Task Tick(CancellationToken stoppingToken)
    {
        var sw = new Stopwatch();
        sw.Start();
        
        try
        {
            var logger = ModBase.ServiceProvider.CreateLogger<ConstructHandleListQueryWorker>();

            var constructHandleRepository = ModBase.ServiceProvider.GetRequiredService<IConstructHandleRepository>();

            var items = await constructHandleRepository.FindActiveHandlesAsync();

            // ConstructBehaviorLoop.ConstructHandles.Clear();
            foreach (var item in items)
            {
                var failed = ConstructBehaviorLoop.ConstructHandles.TryAdd(item.ConstructId, item);

                if (failed)
                {
                    logger.LogError("Failed to add item {ConstructId}", item.ConstructId);
                }
            }

            var deadConstructHandles = ConstructBehaviorLoop.ConstructHandleHeartbeat
                .Where(x => DateTime.UtcNow - x.Value > TimeSpan.FromMinutes(30));

            foreach (var kvp in deadConstructHandles)
            {
                if (stoppingToken.IsCancellationRequested) return;
                
                ConstructBehaviorLoop.ConstructHandles.TryRemove(kvp.Key, out _);
                ConstructBehaviorLoop.ConstructHandleHeartbeat.TryRemove(kvp.Key, out _);
                logger.LogWarning("Removed Construct Handle {Construct} that failed to be removed", kvp.Key);
            }
        }
        catch (Exception e)
        {
            var logger = ModBase.ServiceProvider.CreateLogger<ConstructHandleListQueryWorker>();
            logger.LogError(e, "Failed to Query Construct Handles");
        }
        
        StatsRecorder.Record(nameof(ConstructHandleListQueryWorker), sw.ElapsedMilliseconds);
    }
}