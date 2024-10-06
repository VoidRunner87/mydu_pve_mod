using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Services;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class CleanupLoop(TimeSpan loopTimer) : ModBase
{
    public override async Task Loop()
    {
        while (true)
        {
            try
            {
                await Tick();
                await Task.Delay(loopTimer);
            }
            catch (Exception e)
            {
                var logger = ServiceProvider.CreateLogger<CleanupLoop>();
                logger.LogError(e, "Failed Cleanup Loop");
            }
            
        }
    }

    private async Task Tick()
    {
        var maxIterationsPerCycle = 50;
        var counter = 0;

        var sw = new Stopwatch();
        sw.Start();
        
        var logger = ServiceProvider.CreateLogger<CleanupLoop>();
        var constructService = ServiceProvider.GetRequiredService<IConstructService>();
        var constructHandleRepository = ServiceProvider.GetRequiredService<IConstructHandleRepository>();
        
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
                await Task.Delay(500);
                
                var dequeued = ConstructsPendingDelete.Data.TryDequeue(out _);
                
                logger.LogInformation("Cleaned up {Construct} | DQed={Dequeued}", constructId, dequeued);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to Cleanup {Construct}", constructId);
            }

            counter++;
        }
        
        logger.LogInformation("Cleanup Total = {Time}ms", sw.ElapsedMilliseconds);
        
        RecordHeartBeat();
    }
}