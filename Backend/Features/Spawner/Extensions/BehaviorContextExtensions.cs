using Mod.DynamicEncounters.Features.Spawner.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Extensions;

public static class BehaviorContextExtensions
{
    /// <summary>
    /// Checks if auto set move position is enabled before setting the value. If not noops
    /// </summary>
    /// <param name="context"></param>
    /// <param name="position"></param>
    public static void SetAutoTargetMovePosition(this BehaviorContext context, Vec3 position)
    {
        if (context.IsAutoTargetMovePositionEnabled())
        {
            context.SetTargetMovePosition(position);
        }
    }

    /// <summary>
    /// Checks if auto target construct is enabled. If not noops
    /// </summary>
    /// <param name="context"></param>
    /// <param name="constructId"></param>
    public static void SetAutoTargetConstructId(this BehaviorContext context, ulong? constructId)
    {
        if (context.IsAutoSelectAttackTargetConstructEnabled())
        {
            context.SetTargetConstructId(constructId);
        }
    }
    
    public static void DisableAutoTargetMovePosition(this BehaviorContext context)
    {
        context.SetProperty(BehaviorContext.AutoTargetMovePositionEnabledProperty, false);
    }
    
    public static void DisableAutoSelectAttackTargetConstruct(this BehaviorContext context)
    {
        context.SetProperty(BehaviorContext.AutoSelectAttackTargetConstructProperty, false);
    }

    public static void EnableAutoTargetMovePosition(this BehaviorContext context)
    {
        context.RemoveProperty(BehaviorContext.AutoTargetMovePositionEnabledProperty);
    }
    
    public static void EnableAutoSelectAttackTargetConstruct(this BehaviorContext context)
    {
        context.RemoveProperty(BehaviorContext.AutoSelectAttackTargetConstructProperty);
    }

    public static bool IsAutoTargetMovePositionEnabled(this BehaviorContext context)
    {
        context.TryGetProperty(
            BehaviorContext.AutoTargetMovePositionEnabledProperty,
            out var enabled,
            true
        );

        return enabled;
    }
    
    public static bool IsAutoSelectAttackTargetConstructEnabled(this BehaviorContext context)
    {
        context.TryGetProperty(
            BehaviorContext.AutoSelectAttackTargetConstructProperty,
            out var enabled,
            true
        );

        return enabled;
    }
}