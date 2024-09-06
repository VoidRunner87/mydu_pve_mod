using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ScriptItem
{
    public string Name { get; set; }
    public List<ScriptActionItem> Actions { get; set; }
}