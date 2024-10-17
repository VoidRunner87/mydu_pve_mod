using System;

namespace Mod.DynamicEncounters.Overrides.ApiClient;

public static class PveModBaseUrl
{
    public static string GetBaseUrl()
    {
        var baseUrl = Environment.GetEnvironmentVariable("DYNAMIC_ENCOUNTERS_URL") ??
                      "http://mod_dynamic_encounters:8080";

        return baseUrl;
    }
}