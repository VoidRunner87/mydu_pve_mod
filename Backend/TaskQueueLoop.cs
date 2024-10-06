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
    public override Task Start()
    {
        var provider = ServiceProvider;
        var logger = provider.CreateLogger<TaskQueueLoop>();
        var taskQueueService = provider.GetRequiredService<ITaskQueueService>();
        var featureService = provider.GetRequiredService<IFeatureReaderService>();

        var taskCompletionSource = new TaskCompletionSource();
        
        var timer = new Timer(5000);
        timer.Elapsed += async (sender, args) =>
        {
            try
            {
                var isEnabled = await featureService.GetEnabledValue<TaskQueueLoop>(false);

                if (isEnabled)
                {
                    await taskQueueService.ProcessQueueMessages();
                }
                
                RecordHeartBeat();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to execute {Name}", nameof(TaskQueueLoop));
            }
        };
        
        timer.Start();

        // It will never complete because we're not setting result
        return taskCompletionSource.Task;
    }
}