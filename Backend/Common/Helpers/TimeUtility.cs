using System;

namespace Mod.DynamicEncounters.Common.Helpers;

public static class TimeUtility
{
    public static long GetTimeSnapped(DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
    {
        return (long)(Math.Round(dateTimeOffset.ToUnixTimeSeconds() / timeSpan.TotalSeconds) * timeSpan.TotalSeconds);
    }
}