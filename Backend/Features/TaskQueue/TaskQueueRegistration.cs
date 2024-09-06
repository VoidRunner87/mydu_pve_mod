using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Features.TaskQueue.Repository;
using Mod.DynamicEncounters.Features.TaskQueue.Services;

namespace Mod.DynamicEncounters.Features.TaskQueue;

public static class TaskQueueRegistration
{
    public static void RegisterTaskQueue(this IServiceCollection services)
    {
        services.AddSingleton<ITaskQueueRepository, TaskQueueRepository>();
        services.AddSingleton<ITaskQueueService, TaskQueueService>();
    }
}