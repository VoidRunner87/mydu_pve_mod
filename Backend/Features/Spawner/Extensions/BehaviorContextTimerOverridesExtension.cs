using System;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Extensions;

public static class BehaviorContextTimerOverridesExtension
{
    public static void SetTimerValue<T>(this BehaviorContext context, string name, T value, TimeSpan durationTimeSpan)
    {
        context.PropertyOverrides.Set(
            name,
            new BehaviorContext.TimerPropertyValue(
                DateTime.UtcNow + durationTimeSpan,
                value
            )
        );
    }

    public static T GetOverrideOrDefault<T>(this BehaviorContext context, string name, T defaultValue)
    {
        var value = context.PropertyOverrides
            .GetOrDefault(
                name, 
                new BehaviorContext.TimerPropertyValue(
                    DateTime.UtcNow, 
                    context.Properties.GetOrDefault(name, defaultValue)
                )
            );

        return (T)value.Value;
    }
}