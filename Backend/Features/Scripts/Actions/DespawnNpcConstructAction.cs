using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class DespawnNpcConstructAction : IScriptAction
{
    public const string ActionName = "despawn";
    
    public string GetKey() => Name;

    public string Name => ActionName;
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;

        var logger = provider.CreateLogger<DespawnNpcConstructAction>();
        
        if (!context.ConstructId.HasValue)
        {
            logger.LogError("No construct id on context to execute this action");
            return ScriptActionResult.Failed();
        }
        
        var orleans = provider.GetOrleans();

        var spatialHashRepository = provider.GetRequiredService<IConstructSpatialHashRepository>();
        var constructHandleRepository = provider.GetRequiredService<IConstructHandleRepository>();

        var constructInfoGrain = orleans.GetConstructInfoGrain(context.ConstructId.Value);
        var constructInfo = await constructInfoGrain.Get();
        
        var handleItem = await constructHandleRepository.FindByConstructIdAsync(context.ConstructId.Value);
        if (handleItem == null)
        {
            logger.LogWarning("No handle found for Construct {Construct}. Aborting", context.ConstructId.Value);
            return ScriptActionResult.Failed();
        }
        
        var owner = constructInfo.mutableData.ownerId;
        if (handleItem.OriginalOwnerPlayerId != owner.playerId || handleItem.OriginalOrganizationId != owner.organizationId)
        {
            logger.LogInformation("Prevented Despawn of NPC - Ownership is different than initial Spawn.");
            return ScriptActionResult.Successful();
        }
        
        var playerConstructs = await spatialHashRepository.FindPlayerLiveConstructsOnSector(context.Sector);

        if (playerConstructs.Any())
        {
            logger.LogInformation("Aborting Despawn of NPC. Players Nearby");
            return ScriptActionResult.Failed();
        }

        try
        {
            var parentingGrain = orleans.GetConstructParentingGrain();
            await parentingGrain.DeleteConstruct(context.ConstructId.Value, hardDelete: true);
        
            logger.LogInformation("Deleted NPC construct {ConstructId}", context.ConstructId.Value);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Failed to delete NPC construct {Construct}", context.ConstructId.Value);
            return ScriptActionResult.Failed();
        }
        
        return ScriptActionResult.Successful();
    }
}