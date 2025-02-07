using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Market.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
using NQutils.Def;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("recipes")]
public class RecipesController : Controller
{
    private readonly IRecipes _recipes =
        ModBase.ServiceProvider.GetRequiredService<IRecipes>();

    private readonly IGameplayBank _bank =
        ModBase.ServiceProvider.GetGameplayBank();

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Get()
    {
        var recipes = await _recipes.GetAllRecipes();

        var list = new List<RecipeViewModel>();

        foreach (var recipe in recipes)
        {
            var firstProduct = recipe.products.First();
            var def = _bank.GetDefinition(firstProduct.itemId);

            if (def is not { BaseObject: BaseItem baseItem })
            {
                continue;
            }

            list.Add(new RecipeViewModel
            {
                Tier = baseItem.level,
                Nanopack = recipe.nanocraftable,
                Volume = _bank.GetBaseObject<BaseItem>(recipe.producers.First())!.unitVolume,
                Industry = _bank.GetDefinition(recipe.producers.First())?.BaseObject.DisplayName!,
                Byproducts = recipe.products.ToDictionary(
                    k => _bank.GetDefinition(k.itemId)?.Name,
                    v =>
                    {
                        IItemQuantity quantity = _bank.GetBaseObject<BaseItem>(v.itemId)?.inventoryType == "material"
                            ? new MaterialQuantity(v.quantity.quantity)
                            : new Quantity(v.quantity.quantity);

                        return quantity.GetReadableValue();
                    }
                ),
                Input = recipe.ingredients.ToDictionary(
                    k => _bank.GetDefinition(k.itemId)?.Name,
                    v =>
                    {
                        IItemQuantity quantity = _bank.GetBaseObject<BaseItem>(v.itemId)?.inventoryType == "material"
                            ? new MaterialQuantity(v.quantity.quantity)
                            : new Quantity(v.quantity.quantity);

                        return quantity.GetReadableValue();
                    }),
                Time = (long)recipe.time,
                Type = _bank.GetBaseObject<BaseItem>(firstProduct.itemId)!.DisplayParent
            });
        }

        return Ok(list);
    }

    public class RecipeViewModel
    {
        [JsonProperty("tier")] public int Tier { get; set; }
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("volume")] public double Volume { get; set; }

        [JsonProperty("outputQuantity")] public double OutputQuantity { get; set; }

        [JsonProperty("time")] public long Time { get; set; }

        [JsonProperty("byproducts")] public Dictionary<string, double> Byproducts { get; set; } = [];

        [JsonProperty("input")] public Dictionary<string, double> Input { get; set; } = [];

        [JsonProperty("nanopack")] public bool Nanopack { get; set; }

        [JsonProperty("industry")] public string Industry { get; set; }
    }
}