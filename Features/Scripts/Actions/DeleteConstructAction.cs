using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

/// <summary>
/// No Validation Delete Construct
/// </summary>
/// <param name="constructId"></param>
public class DeleteConstructAction(ulong constructId) : IScriptAction
{
    public string GetKey() => Name;

    public string Name => Guid.NewGuid().ToString();
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;

        var logger = provider.CreateLogger<DeleteConstructAction>();
        var orleans = provider.GetOrleans();

        try
        {
            var parentingGrain = orleans.GetConstructParentingGrain();
            await parentingGrain.DeleteConstruct(constructId, hardDelete: true);
        
            logger.LogInformation("Deleted construct {ConstructId}", constructId);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Failed to delete construct {Construct}", constructId);
            return ScriptActionResult.Failed();
        }
        
        return ScriptActionResult.Successful();
    }
}