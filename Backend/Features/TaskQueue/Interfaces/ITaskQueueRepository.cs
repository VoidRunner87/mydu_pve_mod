using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.TaskQueue.Data;

namespace Mod.DynamicEncounters.Features.TaskQueue.Interfaces;

public interface ITaskQueueRepository
{
    Task AddAsync(TaskQueueItem item);
    Task<IEnumerable<TaskQueueItem>> FindNextAsync(int quantity);
    Task DeleteAsync(Guid id);
    Task TagCompleted(Guid id);
    Task TagFailed(Guid id);
}