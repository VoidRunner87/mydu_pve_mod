using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class TaskQueueLoop : ModBase
{
    public override async Task Loop()
    {
        while (true)
        {
            try
            {
                await Tick();
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            catch (Exception e)
            {
                var logger = ServiceProvider.CreateLogger<TaskQueueLoop>();
                logger.LogError(e, "Failed Task Queue Loop");
            }
        }
    }

    private async Task Tick()
    {
        try
        {
            var provider = ServiceProvider;
            var taskQueueService = provider.GetRequiredService<ITaskQueueService>();
            var featureService = provider.GetRequiredService<IFeatureReaderService>();
            
            var isEnabled = await featureService.GetEnabledValue<TaskQueueLoop>(false);

            if (isEnabled)
            {
                await taskQueueService.ProcessQueueMessages();
            }
                
            RecordHeartBeat();
        }
        catch (Exception e)
        {
            var logger = ServiceProvider.CreateLogger<TaskQueueLoop>();
            logger.LogError(e, "Failed to execute {Name}", nameof(TaskQueueLoop));
        }
    }
}