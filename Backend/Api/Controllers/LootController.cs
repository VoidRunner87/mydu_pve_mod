using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;

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

        await itemSpawner.SpawnFuel(
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
                MaxBudget = budget
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
                MaxBudget = budget
            }
        );

        var itemSpawner = provider.GetRequiredService<IItemSpawnerService>();

        await itemSpawner.SpawnItems(new SpawnItemOnRandomContainersCommand((ulong)constructId, itemBag));

        return Ok();
    }
}