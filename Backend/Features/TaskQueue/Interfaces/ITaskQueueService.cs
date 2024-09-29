using System;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.TaskQueue.Interfaces;

public interface ITaskQueueService
{
    Task ProcessQueueMessages();
    Task EnqueueScript(ScriptActionItem script, DateTime? deliveryAt);
}