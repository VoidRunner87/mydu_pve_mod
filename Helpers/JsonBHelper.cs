namespace Mod.DynamicEncounters.Helpers;

public static class JsonBHelper
{
    public static string AsJsonB(this string value)
        => $"\"{value}\"";
}