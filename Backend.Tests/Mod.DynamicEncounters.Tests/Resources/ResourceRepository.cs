using System.Reflection;
using Mod.DynamicEncounters.Common.Helpers;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Tests.Resources;

public static class ResourceRepository
{
    private static string BaseNamespace => "Mod.DynamicEncounters.Tests.Resources";

    public static ScriptActionItem TagSectorAsActiveScript => JsonConvert.DeserializeObject<ScriptActionItem>(
        ResourceLoader.GetContents(Assembly.GetExecutingAssembly(),
            $"{BaseNamespace}.tag-sector-as-active.json"))!;
}