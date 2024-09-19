using System;

namespace Mod.DynamicEncounters.Features.Events.Data;

public class EventTriggerItem(string eventName, string onTriggerScript)
{
    public Guid Id { get; set; }
    public string EventName { get; set; } = eventName;
    public long? FactionId { get; set; }
    public double MinTriggerValue { get; set; } = 0;
    public string OnTriggerScript { get; set; } = onTriggerScript;

    public bool ShouldTrigger(double value)
    {
        return value >= MinTriggerValue;
    }
}