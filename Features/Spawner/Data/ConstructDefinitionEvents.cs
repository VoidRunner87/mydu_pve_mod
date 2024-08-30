using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class ConstructDefinitionEvents
{
    public List<ScriptActionItem> OnShieldHalf { get; set; } = [];
    public List<ScriptActionItem> OnShieldLow { get; set; } = [];
    public List<ScriptActionItem> OnShieldDown { get; set; } = [];
    public List<ScriptActionItem> OnCoreStressHigh { get; set; } = [];
    public List<ScriptActionItem> OnDestruction { get; set; } = [];
}