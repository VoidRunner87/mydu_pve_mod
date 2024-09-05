using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IScriptActionFactory
{
    IScriptAction Create(ScriptActionItem scriptActionItem);

    IScriptAction Create(IEnumerable<ScriptActionItem> scriptActionItem);

    IEnumerable<string> GetAllActions();
}