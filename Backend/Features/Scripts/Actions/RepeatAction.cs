using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, VisibleOnUI = true)]
public class RepeatAction(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "repeat";

    public string Name { get; } = Guid.NewGuid().ToString();
    public string GetKey() => Name;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var repeatCount = (int)actionItem.Value;
        var provider = context.ServiceProvider;
        var scriptFactory = provider.GetRequiredService<IScriptActionFactory>();

        var action = scriptFactory.Create(actionItem.Actions);
        
        for (var i = 0; i < repeatCount; i++)
        {
            await action.ExecuteAsync(
                new ScriptContext(
                    provider,
                    context.FactionId,
                    context.PlayerIds,
                    context.Sector
                )
                {
                    ConstructId = context.ConstructId
                }
            );
        }

        return ScriptActionResult.Successful();
    }
}