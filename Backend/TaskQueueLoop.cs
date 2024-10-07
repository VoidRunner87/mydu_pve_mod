using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Threads;

namespace Mod.DynamicEncounters;

public class TaskQueueLoop(IThreadManager tm, CancellationToken ct) : ThreadHandle(ThreadId.TaskQueue, tm, ct)
{
    public override async Task Tick()
    {
        try
        {
            var provider = ModBase.ServiceProvider;
            var taskQueueService = provider.GetRequiredService<ITaskQueueService>();
            var featureService = provider.GetRequiredService<IFeatureReaderService>();
            
            var isEnabled = await featureService.GetEnabledValue<TaskQueueLoop>(false);

            if (isEnabled)
            {
                await taskQueueService.ProcessQueueMessages();
            }
                
            ReportHeartbeat();
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
        catch (Exception e)
        {
            var logger = ModBase.ServiceProvider.CreateLogger<TaskQueueLoop>();
            logger.LogError(e, "Failed to execute {Name}", nameof(TaskQueueLoop));
        }
    }
}