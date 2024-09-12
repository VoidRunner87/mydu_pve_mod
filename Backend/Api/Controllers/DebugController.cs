using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("debug")]
public class DebugController : Controller
{
    [HttpGet]
    [Route("")]
    public IActionResult GetItemDefs()
    {
        var bank = ModBase.ServiceProvider.GetGameplayBank();
        var items = bank.GetDefinitions()
            .Select(x => new
            {
                x.Id,
                x.Name
            });

        return Ok(items);
    }

    [HttpPut]
    [Route("{constructId:long}/{tags}/{budget:int}")]
    public async Task<IActionResult> Run(long constructId, string tags, int budget)
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

        await itemSpawner.SpawnItems(new SpawnItemCommand((ulong)constructId, itemBag));

        return Ok();
    }
}