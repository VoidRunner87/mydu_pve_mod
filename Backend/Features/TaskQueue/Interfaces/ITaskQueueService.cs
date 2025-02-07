using System;
using System.Threading;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.TaskQueue.Interfaces;

public interface ITaskQueueService
{
    Task ProcessQueueMessages(CancellationToken cancellationToken);
    Task EnqueueScript(ScriptActionItem script, DateTime? deliveryAt);
}