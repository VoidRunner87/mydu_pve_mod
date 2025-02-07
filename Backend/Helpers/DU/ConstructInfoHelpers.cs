using Mod.DynamicEncounters.Features.Spawner.Data;
using NQ;

namespace Mod.DynamicEncounters.Helpers.DU;

public static class ConstructInfoHelpers
{
    public static bool IsAbandoned(this ConstructInfo info)
    {
        return info.mutableData.ownerId.playerId == 0;
    }

    public static bool HasShield(this ConstructInfo info)
    {
        return info.mutableData.shieldState.hasShield;
    }

    public static bool IsShieldDown(this BehaviorContext context)
    {
        if (!context.HasShield)
        {
            return true;
        }

        if (!context.IsShieldActive)
        {
            return true;
        }

        if (context.ShieldPercent <= 0)
        {
            return true;
        }

        return false;
    }
    
    public static bool IsShieldLowerThanHalf(this BehaviorContext context)
    {
        if (context.IsShieldDown())
        {
            return true;
        }

        return context.ShieldPercent < 0.5;
    }
    
    public static bool IsShieldLowerThan25(this BehaviorContext context)
    {
        if (context.IsShieldDown())
        {
            return true;
        }

        return context.ShieldPercent < 0.25;
    }
}