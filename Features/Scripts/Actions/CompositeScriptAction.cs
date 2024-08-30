using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class CompositeScriptAction(string name, IEnumerable<IScriptAction> actions) : IScriptAction
{
    public string Name { get; } = name;
    
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