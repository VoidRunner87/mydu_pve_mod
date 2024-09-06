using System;

namespace Mod.DynamicEncounters.Features.TaskQueue.Data;

public class TaskQueueItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Command { get; set; }
    public DateTime DeliveryAt { get; set; } = DateTime.UtcNow;
    public object Data { get; set; }
    public string Status { get; set; }
}