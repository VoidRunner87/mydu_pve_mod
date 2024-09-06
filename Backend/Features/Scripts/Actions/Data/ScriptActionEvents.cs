using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ScriptActionEvents
{
    public List<ScriptActionItem> OnLoad { get; set; } = new();
    public List<ScriptActionItem> OnSectorEnter { get; set; } = new();
}