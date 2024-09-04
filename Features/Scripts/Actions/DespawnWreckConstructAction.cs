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

public class DespawnWreckConstructAction : IScriptAction
{
    public string GetKey() => Name;

    public string Name { get; } = Guid.NewGuid().ToString();
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;

        var logger = provider.CreateLogger<DespawnWreckConstructAction>();
        
        if (!context.ConstructId.HasValue)
        {
            logger.LogError("No construct id on context to execute this action");
            return ScriptActionResult.Failed();
        }
        
        var orleans = provider.GetOrleans();

        var constructHandleRepository = provider.GetRequiredService<IConstructHandleRepository>();
        var sectorInstanceRepository = provider.GetRequiredService<ISectorInstanceRepository>();
        
        var sector = await sectorInstanceRepository.FindBySector(context.Sector);
        if (sector is { StartedAt: not null })
        {
            logger.LogWarning("Construct was already discovered: {Construct}. Aborting", context.ConstructId.Value);
            return ScriptActionResult.Successful();
        }

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
        
        try
        {
            var parentingGrain = orleans.GetConstructParentingGrain();
            await parentingGrain.DeleteConstruct(context.ConstructId.Value, hardDelete: true);
        
            logger.LogInformation("Deleted Wreck construct {ConstructId}", context.ConstructId.Value);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Failed to delete Wreck construct {Construct}", context.ConstructId.Value);
            return ScriptActionResult.Failed();
        }
        
        return ScriptActionResult.Successful();
    }
}