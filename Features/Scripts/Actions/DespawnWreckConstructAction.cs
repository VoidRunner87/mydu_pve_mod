using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class DespawnWreckConstructAction(ulong constructId) : IScriptAction
{
    public string GetKey() => Name;

    public string Name { get; } = Guid.NewGuid().ToString();
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;

        var logger = provider.CreateLogger<DespawnWreckConstructAction>();
        var orleans = provider.GetOrleans();

        var constructHandleRepository = provider.GetRequiredService<IConstructHandleRepository>();
        var sectorInstanceRepository = provider.GetRequiredService<ISectorInstanceRepository>();
        
        var sector = await sectorInstanceRepository.FindBySector(context.Sector);
        if (sector is { StartedAt: not null })
        {
            logger.LogWarning("Construct was already discovered: {Construct}. Aborting", constructId);
            return ScriptActionResult.Successful();
        }

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
        
        try
        {
            var parentingGrain = orleans.GetConstructParentingGrain();
            await parentingGrain.DeleteConstruct(constructId, hardDelete: true);
        
            logger.LogInformation("Deleted Wreck construct {ConstructId}", constructId);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Failed to delete Wreck construct {Construct}", constructId);
            return ScriptActionResult.Failed();
        }
        
        return ScriptActionResult.Successful();
    }
}