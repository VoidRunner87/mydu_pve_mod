using System;

namespace Mod.DynamicEncounters.Helpers;

public static class TimeSpanHelpers
{
    /// <summary>
    /// Converts a TimeSpan to a PostgreSQL interval string.
    /// </summary>
    /// <param name="timeSpan">The TimeSpan to convert.</param>
    /// <returns>A string representation of the TimeSpan in PostgreSQL interval format.</returns>
    public static string ToPostgresInterval(this TimeSpan timeSpan)
    {
        // Construct the interval parts
        var days = timeSpan.Days;
        var hours = timeSpan.Hours;
        var minutes = timeSpan.Minutes;
        var seconds = timeSpan.Seconds + timeSpan.Milliseconds / 1000.0;

        // Build the interval string
        string interval = "";

        if (days > 0)
            interval += $"{days} days ";

        if (hours > 0)
            interval += $"{hours} hours ";

        if (minutes > 0)
            interval += $"{minutes} minutes ";

        if (seconds > 0 || interval == "")
            interval += $"{seconds} seconds";

        // Trim trailing space and return
        return interval.Trim();
    }
}