namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IConstructEvents
{
    IScriptAction OnShieldHalfAction { get; set; }
    IScriptAction OnShieldLowAction { get; set; }
    IScriptAction OnShieldDownAction { get; set; }
    IScriptAction OnCoreStressHigh { get; set; }
    IScriptAction OnDestruction { get; set; }
}