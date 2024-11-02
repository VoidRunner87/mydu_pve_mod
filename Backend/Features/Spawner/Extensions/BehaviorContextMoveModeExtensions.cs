using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Extensions;

public static class BehaviorContextMoveModeExtensions
{
    public static void SetMoveModeWaypoint(this BehaviorContext context)
    {
        context.SetProperty(BehaviorContext.MoveModeProperty, "waypoint");
    }

    public static bool IsMoveModeWaypoint(this BehaviorContext context)
    {
        return context.TryGetProperty(
            BehaviorContext.MoveModeProperty, 
            out var mode, 
            string.Empty
        ) && mode == "waypoint";
    }
    
    public static bool IsMoveModeDefault(this BehaviorContext context)
    {
        return context.TryGetProperty(
            BehaviorContext.MoveModeProperty, 
            out var mode, 
            string.Empty
        ) && mode == string.Empty;
    }
    
    public static void SetBraking(this BehaviorContext context, bool value)
    {
        context.SetProperty(BehaviorContext.BrakingProperty, value);
    }
    
    public static bool IsBraking(this BehaviorContext context)
    {
        return context.TryGetProperty(
            BehaviorContext.BrakingProperty, 
            out var braking, 
            false
        ) && braking;
    }

    public static void SetMoveModeDefault(this BehaviorContext context)
    {
        context.RemoveProperty(BehaviorContext.MoveModeProperty);
    }
}