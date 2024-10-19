using System;

namespace Mod.DynamicEncounters.Overrides.Common;

public static class Config
{
    public static string GetPveModBaseUrl()
    {
        return Environment.GetEnvironmentVariable("DYNAMIC_ENCOUNTERS_URL") ??
               "http://mod_dynamic_encounters:8080";
    }
}