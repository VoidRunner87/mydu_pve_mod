using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, Description = Description)]
public class SpawnLootForConstruct(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "spawn-loot";
    public const string Description = "Spawns Loot for the construct in context";
    
    public string Name => ActionName;
    public string GetKey() => Name;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        if (!context.ConstructId.HasValue)
        {
            return ScriptActionResult.Failed();
        }
        
        var provider = context.ServiceProvider;
        var logger = provider.CreateLogger<SpawnLootForConstruct>();

        var lootGeneratorService = provider.GetRequiredService<ILootGeneratorService>();
        var itemBagData = await lootGeneratorService.GenerateAsync(
            new LootGenerationArgs
            {
                Tags = actionItem.Tags,
                MaxBudget = actionItem.Value
            }
        );

        var itemSpawnerService = provider.GetRequiredService<IItemSpawnerService>();
        await itemSpawnerService.SpawnItems(
            new SpawnItemOnRandomContainersCommand(context.ConstructId.Value, itemBagData)
        );
        
        logger.LogInformation("Spawned Loot for Construct {Construct}", context.ConstructId);

        try
        {
            var elementReplacer = provider.GetRequiredService<IElementReplacerService>();
            foreach (var replace in itemBagData.ElementsToReplace)
            {
                for (var i = 0; i < replace.Quantity; i++)
                {
                    await elementReplacer.ReplaceSingleElementAsync(
                        context.ConstructId.Value,
                        replace.ElementName,
                        replace.ReplaceElementName
                    );
                }
            }
        
            logger.LogInformation("Processed Element Replacements");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to Replace Elements. Rule {Rule}", JsonConvert.SerializeObject(itemBagData.ElementsToReplace));
        }

        return ScriptActionResult.Successful();
    }
}