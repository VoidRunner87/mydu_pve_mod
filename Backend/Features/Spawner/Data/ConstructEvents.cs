using Mod.DynamicEncounters.Features.Scripts.Actions;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class ConstructEvents : IConstructEvents
{
    public IScriptAction OnShieldHalfAction { get; set; } = new NullScriptAction();
    public IScriptAction OnShieldLowAction { get; set; } = new NullScriptAction();
    public IScriptAction OnShieldDownAction { get; set; } = new NullScriptAction();
    public IScriptAction OnCoreStressHigh { get; set; } = new NullScriptAction();
    public IScriptAction OnDestruction { get; set; } = new NullScriptAction();
}