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
    [Route("{constructId:long}/{item}/{amount:long}")]
    public async Task<IActionResult> Run(long constructId, string item, long amount)
    {
        var itemSpawner = ModBase.ServiceProvider.GetRequiredService<IItemSpawnerService>();
        await itemSpawner.SpawnItems(
            new SpawnItemCommand(
                (ulong)constructId,
                new ItemData
                {
                    Entries = new[]
                    {
                        new ItemData.ItemAndQuantity(item, amount)
                    }
                }
            )
        );

        return Ok();
    }
}