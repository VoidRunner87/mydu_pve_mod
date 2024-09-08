using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, VisibleOnUI = false)]
public class RunScriptAction(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "script";
    
    public string Name { get; } = Guid.NewGuid().ToString();

    public Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var scriptService = provider.GetRequiredService<IScriptService>();

        return scriptService.ExecuteScriptAsync(actionItem.Script, context);
    }

    public string GetKey() => Name;
}