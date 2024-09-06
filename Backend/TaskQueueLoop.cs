using System;
using System.Threading.Tasks;
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
        var provider = ServiceProvider;
        var logger = provider.CreateLogger<TaskQueueLoop>();
        var taskQueueService = provider.GetRequiredService<ITaskQueueService>();
        var featureService = provider.GetRequiredService<IFeatureReaderService>();
        
        try
        {
            while (true)
            {
                await Task.Delay(5000);
                var isEnabled = await featureService.GetEnabledValue<TaskQueueLoop>(false);

                if (isEnabled)
                {
                    await taskQueueService.ProcessQueueMessages(Bot);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to execute {Name}", nameof(TaskQueueLoop));
            // TODO implement alerting on too many failures
        }
    }
}