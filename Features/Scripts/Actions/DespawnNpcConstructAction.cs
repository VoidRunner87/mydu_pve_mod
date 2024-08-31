using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class DespawnNpcConstructAction(ulong constructId) : IScriptAction
{
    public string GetKey() => Name;

    public string Name => Guid.NewGuid().ToString();
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;

        var logger = provider.CreateLogger<DespawnNpcConstructAction>();
        var orleans = provider.GetOrleans();

        var spatialHashRepository = provider.GetRequiredService<IConstructSpatialHashRepository>();
        var constructHandleRepository = provider.GetRequiredService<IConstructHandleRepository>();

        var constructInfoGrain = orleans.GetConstructInfoGrain(constructId);
        var constructInfo = await constructInfoGrain.Get();
        
        var handleItem = await constructHandleRepository.FindByConstructIdAsync(constructId);
        if (handleItem == null)
        {
            logger.LogWarning("No handle found for Construct {Construct}. Aborting", constructId);
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
            await parentingGrain.DeleteConstruct(constructId, hardDelete: true);
        
            logger.LogInformation("Deleted NPC construct {ConstructId}", constructId);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Failed to delete NPC construct {Construct}", constructId);
            return ScriptActionResult.Failed();
        }
        
        return ScriptActionResult.Successful();
    }
}