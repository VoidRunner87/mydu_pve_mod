namespace Mod.DynamicEncounters.Common;

public static class StringHelpers
{
    public static string Truncate(this string input, int maxChar) 
        => input.Length > maxChar ? input[..maxChar] : input;
}