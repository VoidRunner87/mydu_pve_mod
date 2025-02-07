using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

/// <summary>
/// No Validation Delete Construct
/// </summary>
[ScriptActionName(ActionName)]
public class ReloadConstructAction(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "reload-construct";
    
    public string GetKey() => Name;

    public string Name => ActionName;
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;

        var logger = provider.CreateLogger<DeleteConstructAction>();

        if (!context.ConstructId.HasValue && actionItem.ConstructId > 0)
        {
            context.ConstructId = actionItem.ConstructId;
        }
        
        if (!context.ConstructId.HasValue)
        {
            logger.LogError("No construct id on context to execute this action");
            return ScriptActionResult.Failed();
        }
        
        var orleans = provider.GetOrleans();

        try
        {
            var parentingGrain = orleans.GetConstructParentingGrain();
            await parentingGrain.ReloadConstruct(context.ConstructId.Value);
        
            logger.LogInformation("Reloaded construct {ConstructId}", context.ConstructId.Value);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Failed to reload construct {Construct}", context.ConstructId.Value);
            return ScriptActionResult.Failed();
        }
        
        return ScriptActionResult.Successful();
    }
}