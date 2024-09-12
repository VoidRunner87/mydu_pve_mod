using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, Description = Description)]
public class Repeat(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "repeat";
    public const string Description = "Repeats an action N number of times";
    
    public string Name { get; } = Guid.NewGuid().ToString();
    public string GetKey() => Name;
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var actionFactory = provider.GetRequiredService<IScriptActionFactory>();
        
        for (var i = 0; i < actionItem.Value; i++)
        {
            var action = actionFactory.Create(actionItem.Actions);
            await action.ExecuteAsync(
                context
            );
        }
        
        return ScriptActionResult.Successful();
    }
}