using System.Collections.Generic;
using NQ;

namespace Mod.DynamicEncounters.Helpers.DU;

public static class ElementInfoHelpers
{
    public static bool IsCoreDestroyed(this ElementInfo elementInfo)
    {
        if (TryGetBoolValue(elementInfo.properties, "destroyed", out var destroyed) && destroyed)
        {
            return true;
        }

        return GetCoreStressPercentage(elementInfo) >= 0.999999;
    }
    
    public static bool IsCoreStressHigh(this ElementInfo elementInfo)
    {
        return GetCoreStressPercentage(elementInfo) > 0.80;
    }
    
    public static float GetCoreStressPercentage(this ElementInfo elementInfo)
    {
        if (elementInfo.properties.TryGetDoubleValue("stressMaxHp", out var stressMaxHp))
        {
            if (elementInfo.properties.TryGetDoubleValue("stressCurrentHp", out var stressCurrentHp))
            {
                // Ship without Voxels - then no CCS
                if (stressMaxHp == 0)
                {
                    return 0;
                }
                
                if (stressMaxHp <= 0)
                {
                    return 1;
                }
                
                return (float)((float) stressCurrentHp / stressMaxHp);
            }
        }

        return 1;
    }

    public static bool TryGetDoubleValue(this Dictionary<string, PropertyValue> propertyValues, string name, out double value)
    {
        if (propertyValues.TryGetValue(name, out var propertyValue))
        {
            value = propertyValue.doubleValue;
            return true;
        }

        value = default;
        return false;
    }
    
    public static bool TryGetBoolValue(this Dictionary<string, PropertyValue> propertyValues, string name, out bool value)
    {
        if (propertyValues.TryGetValue(name, out var propertyValue))
        {
            value = propertyValue.boolValue;
            return true;
        }

        value = default;
        return false;
    }
}