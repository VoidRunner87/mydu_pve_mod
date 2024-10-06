using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class CompositeScriptAction(IEnumerable<IScriptAction> actions) : IScriptAction
{
    public string Name => "composite";

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        await Task.Yield();
        
        foreach (var action in actions)
        {
            await action.ExecuteAsync(context);
        }

        return ScriptActionResult.Successful();
    }

    public string GetKey() => Name;
}