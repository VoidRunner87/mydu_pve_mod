using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Services;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class CleanupWorker : BackgroundService
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
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception e)
            {
                ModBase.ServiceProvider.CreateLogger<CleanupWorker>()
                    .LogError(e, "{Type} Exception: {Message}", GetType().Name, e.Message);
            }
        }
    }
    
    public async Task Tick(CancellationToken stoppingToken)
    {
        var maxIterationsPerCycle = 50;
        var counter = 0;

        var sw = new Stopwatch();
        sw.Start();

        try
        {
            var logger = ModBase.ServiceProvider.CreateLogger<CleanupWorker>();
            var constructService = ModBase.ServiceProvider.GetRequiredService<IConstructService>();
            var constructHandleRepository = ModBase.ServiceProvider.GetRequiredService<IConstructHandleRepository>();

            while (!stoppingToken.IsCancellationRequested && !ConstructsPendingDelete.Data.IsEmpty)
            {
                if (counter > maxIterationsPerCycle)
                {
                    break;
                }

                if (!ConstructsPendingDelete.Data.TryPeek(out var constructId))
                {
                    continue;
                }

                try
                {
                    await constructService.SoftDeleteAsync(constructId);
                    await constructHandleRepository.DeleteByConstructId(constructId);
                    Thread.Sleep(500);

                    var dequeued = ConstructsPendingDelete.Data.TryDequeue(out _);

                    logger.LogInformation("Cleaned up {Construct} | DQed={Dequeued}", constructId, dequeued);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to Cleanup {Construct}", constructId);
                }

                counter++;
            }

            await constructHandleRepository.CleanupOldDeletedConstructHandles().WaitAsync(stoppingToken);
            
            logger.LogInformation("Cleanup Total = {Time}ms", sw.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            var logger = ModBase.ServiceProvider.CreateLogger<CleanupWorker>();
            logger.LogError(e, "Failed Cleanup");
        }
    }
}