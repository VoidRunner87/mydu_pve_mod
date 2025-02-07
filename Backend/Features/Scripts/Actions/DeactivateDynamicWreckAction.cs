using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class DeactivateDynamicWreckAction : IScriptAction
{
    public const string ActionName = "remove-poi";

    public string GetKey() => Name;

    public string Name => ActionName;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        if (!context.ConstructId.HasValue)
        {
            return ScriptActionResult.Failed();
        }
        
        var provider = context.ServiceProvider;
        var logger = provider.CreateLogger<DeactivateDynamicWreckAction>();

        var constructService = provider.GetRequiredService<IConstructService>();

        await constructService.SetDynamicWreckAsync(context.ConstructId.Value, false);
        
        logger.LogInformation("Construct '{Construct}' was set as dynamic wreck '{Value}'", context.ConstructId.Value, false);
        
        return ScriptActionResult.Successful();
    }
}