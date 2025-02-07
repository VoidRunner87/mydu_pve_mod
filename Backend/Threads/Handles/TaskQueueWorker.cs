using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class TaskQueueWorker : BackgroundService
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
                ModBase.ServiceProvider.CreateLogger<TaskQueueWorker>()
                    .LogError(e, "{Type} Exception: {Message}", GetType().Name, e.Message);
            }
        }
    }
    
    private async Task Tick(CancellationToken stoppingToken)
    {
        try
        {
            var provider = ModBase.ServiceProvider;
            var taskQueueService = provider.GetRequiredService<ITaskQueueService>();

            await taskQueueService.ProcessQueueMessages(stoppingToken);
        }
        catch (Exception e)
        {
            var logger = ModBase.ServiceProvider.CreateLogger<TaskQueueWorker>();
            logger.LogError(e, "Failed to execute {Name}", nameof(TaskQueueWorker));
        }
    }
}