using System;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class GiveQuantaToPlayer : IScriptAction
{
    public string Name { get; } = Guid.NewGuid().ToString();

    public string GetKey() => Name;
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var orleans = provider.GetOrleans();

        await Task.Yield(); // TODO
        
        return ScriptActionResult.Failed();
    }
}