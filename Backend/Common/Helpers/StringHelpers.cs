namespace Mod.DynamicEncounters.Common.Helpers;

public static class StringHelpers
{
    public static string Truncate(this string input, int maxChar) 
        => input.Length > maxChar ? input[..maxChar] : input;
}