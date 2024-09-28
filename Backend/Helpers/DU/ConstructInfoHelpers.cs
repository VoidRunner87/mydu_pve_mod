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

    public static bool IsShieldDown(this ConstructInfo info)
    {
        if (!info.mutableData.shieldState.hasShield)
        {
            return true;
        }

        if (!info.mutableData.shieldState.isActive)
        {
            return true;
        }

        if (info.mutableData.shieldState.shieldHpRatio <= 0)
        {
            return true;
        }

        return false;
    }
    
    public static bool IsShieldLowerThanHalf(this ConstructInfo info)
    {
        if (info.IsShieldDown())
        {
            return true;
        }

        return info.mutableData.shieldState.shieldHpRatio < 0.5;
    }
    
    public static bool IsShieldLowerThan25(this ConstructInfo info)
    {
        if (info.IsShieldDown())
        {
            return true;
        }

        return info.mutableData.shieldState.shieldHpRatio < 0.25;
    }
}