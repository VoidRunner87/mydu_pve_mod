using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, VisibleOnUI = true)]
public class RunScriptAction(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "script";

    public string Name => ActionName;

    public Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var scriptService = provider.GetRequiredService<IScriptService>();

        return scriptService.ExecuteScriptAsync(
            actionItem.Script,
            new ScriptContext(provider, context.FactionId, context.PlayerIds, context.Sector, context.TerritoryId)
            {
                ConstructId = context.ConstructId,
                Properties = context.Properties
            }
        );
    }

    public string GetKey() => Name;
}