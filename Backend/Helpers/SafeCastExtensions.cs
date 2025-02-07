namespace Mod.DynamicEncounters.Helpers;

public static class SafeCastExtensions
{
    /// <summary>
    /// Safely casts an object to the specified type T, or returns the provided default value if the cast fails.
    /// </summary>
    /// <typeparam name="T">The target type to cast to.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <param name="defaultValue">The default value to return if the cast fails.</param>
    /// <returns>The object cast to type T, or the provided default value if the cast fails.</returns>
    public static T SafeCastOrDefault<T>(this object obj, T defaultValue)
    {
        return obj is T castedObj ? castedObj : defaultValue;
    }
}