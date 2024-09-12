using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, Description = Description)]
public class SpawnLootForConstruct(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "spawn-loot";
    public const string Description = "Spawns Loot for the construct in context";
    
    public string Name { get; } = Guid.NewGuid().ToString();
    public string GetKey() => Name;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        if (!context.ConstructId.HasValue)
        {
            return ScriptActionResult.Failed();
        }
        
        var provider = context.ServiceProvider;

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
            new SpawnItemCommand(context.ConstructId.Value, itemBagData)
        );

        return ScriptActionResult.Successful();
    }
}