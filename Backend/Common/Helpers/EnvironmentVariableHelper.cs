using System;

namespace Mod.DynamicEncounters.Common.Helpers;

public static class EnvironmentVariableHelper
{
    /// <summary>
    /// Checks the Environment variable ENVIRONMENT to check if it's production.
    /// If none is defined, assumes production
    /// </summary>
    /// <returns></returns>
    public static bool IsProduction()
        => GetEnvironmentVarOrDefault("ENVIRONMENT", "PROD") == "PROD";
    
    public static string GetEnvironmentVarOrDefault(string varName, string defaultValue)
    {
        var envVar = Environment.GetEnvironmentVariable(varName);

        if (string.IsNullOrEmpty(envVar))
        {
            return defaultValue;
        }

        return envVar;
    }
}