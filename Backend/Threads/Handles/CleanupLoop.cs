using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Services;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class CleanupLoop(IThreadManager tm, CancellationToken ct) : ThreadHandle(ThreadId.Cleanup, tm, ct)
{
    private readonly TimeSpan _timeSpan = TimeSpan.FromSeconds(5);

    public override async Task Tick()
    {
        var maxIterationsPerCycle = 50;
        var counter = 0;

        var sw = new Stopwatch();
        sw.Start();

        try
        {
            var logger = ModBase.ServiceProvider.CreateLogger<CleanupLoop>();
            var constructService = ModBase.ServiceProvider.GetRequiredService<IConstructService>();
            var constructHandleRepository = ModBase.ServiceProvider.GetRequiredService<IConstructHandleRepository>();

            while (!ConstructsPendingDelete.Data.IsEmpty)
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

            await constructHandleRepository.CleanupConstructHandles();
            
            logger.LogInformation("Cleanup Total = {Time}ms", sw.ElapsedMilliseconds);

            ReportHeartbeat();

            Thread.Sleep(_timeSpan);
        }
        catch (Exception e)
        {
            var logger = ModBase.ServiceProvider.CreateLogger<CleanupLoop>();
            logger.LogError(e, "Failed Cleanup");
        }
    }
}