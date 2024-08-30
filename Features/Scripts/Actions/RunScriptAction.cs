using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class RunScriptAction(string script) : IScriptAction
{
    public string Name { get; } = Guid.NewGuid().ToString();
    
    public Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var scriptService = provider.GetRequiredService<IScriptService>();

        return scriptService.ExecuteScriptAsync(script, context);
    }

    public string GetKey() => Name;
}