namespace Mod.DynamicEncounters.Common;

public static class Resources
{
    public static string CommonJs => ResourceLoader
        .GetStringContents("Mod.DynamicEncounters.Resources.common.js");
    public static string CreateRootDivJs => ResourceLoader
        .GetStringContents("Mod.DynamicEncounters.Resources.create-root-div.js");
    public static string NpcAppJs => ResourceLoader
        .GetStringContents("Mod.DynamicEncounters.Resources.npc-app.js");
    public static string NpcAppCss => ResourceLoader
        .GetStringContents("Mod.DynamicEncounters.Resources.npc-app.css");
}