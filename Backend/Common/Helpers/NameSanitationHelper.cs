using System.Text.RegularExpressions;

namespace Mod.DynamicEncounters.Common.Helpers;

public static partial class NameSanitationHelper
{
    public static string SanitizeName(string name)
    {
        return SanitizeNameRegex().Replace(name.ToLower(), "");
    }
    
    [GeneratedRegex("[^a0-z9\\-]")]
    private static partial Regex SanitizeNameRegex();
}