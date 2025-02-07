using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQutils.Def;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("loot")]
public class LootController : Controller
{
    [HttpPut]
    [Route("fuel/spawn/{constructId:long}")]
    public async Task<IActionResult> SpawnRandomFuel(long constructId)
    {
        var provider = ModBase.ServiceProvider;

        var itemSpawner = provider.GetRequiredService<IItemSpawnerService>();

        await itemSpawner.SpawnSpaceFuel(
            new SpawnFuelCommand((ulong)constructId)
        );

        return Ok();
    }

    [HttpPut]
    [Route("simulate/tags/{tags}/budget/{budget:int}")]
    public async Task<IActionResult> SimulateLoot(string tags, int budget)
    {
        var provider = ModBase.ServiceProvider;

        var lootGeneratorService = provider.GetRequiredService<ILootGeneratorService>();
        var itemBag = await lootGeneratorService.GenerateAsync(
            new LootGenerationArgs
            {
                Tags = tags.Split(","),
                MaxBudget = budget,
                Operator = TagOperator.AllTags
            }
        );

        return Ok(new
        {
            itemBag.MaxBudget,
            itemBag.CurrentCost,
            Entries = itemBag.GetEntries()
                .Select(x => new
                {
                    x.ItemName,
                    Quantity = x.Quantity.GetRawQuantity()
                }),
            Elements = itemBag.ElementsToReplace
        });
    }

    [HttpPut]
    [Route("spawn/{constructId:long}/tags/{tags}/budget/{budget:int}")]
    public async Task<IActionResult> SpawnLoot(long constructId, string tags, int budget)
    {
        var provider = ModBase.ServiceProvider;

        var lootGeneratorService = provider.GetRequiredService<ILootGeneratorService>();
        var itemBag = await lootGeneratorService.GenerateAsync(
            new LootGenerationArgs
            {
                Tags = tags.Split(","),
                MaxBudget = budget,
                Operator = TagOperator.AllTags
            }
        );

        var itemSpawner = provider.GetRequiredService<IItemSpawnerService>();

        await itemSpawner.SpawnItems(new SpawnItemOnRandomContainersCommand((ulong)constructId, itemBag));

        return Ok();
    }

    [HttpPost]
    [Route("item-type/{itemTypeName}/tier/{tier:int}")]
    public IActionResult CreateItemTypeLoot(
        string itemTypeName,
        int tier,
        [FromBody] GenerateLootDataRequest request
    )
    {
        var provider = ModBase.ServiceProvider;
        var bank = provider.GetGameplayBank();

        var definition = bank.GetDefinition(itemTypeName);

        if (definition == null)
        {
            return NotFound();
        }

        var items = new List<IGameplayDefinition>();

        EnumerateItemsRecursive(definition, items);

        items = items.Where(x => x.BaseObject.hidden == false)
            .Where(x => x.BaseObject is BaseItem baseItem && baseItem.level == tier)
            .ToList();

        return Ok(items
            .Select(x => new LootDefinitionItem
                .LootItemRule(x.Name)
                {
                    MinQuantity = request.MinQuantity,
                    MaxQuantity = request.MaxQuantity,
                    Chance = 1,
                    MaxSpawnCost = 1
                }));
    }

    private static void EnumerateItemsRecursive(IGameplayDefinition definition, IList<IGameplayDefinition> result)
    {
        if (!definition.GetChildren().Any())
        {
            result.Add(definition);
            return;
        }

        foreach (var child in definition.GetChildren())
        {
            EnumerateItemsRecursive(child, result);
        }
    }

    public class GenerateLootDataRequest
    {
        public long MinQuantity { get; set; } = 1;
        public long MaxQuantity { get; set; } = 1;
    }
}