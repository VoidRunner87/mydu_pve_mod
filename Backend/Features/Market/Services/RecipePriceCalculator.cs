using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Market.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQutils.Def;
using RecipeItem = Mod.DynamicEncounters.Features.Market.Data.RecipeItem;

namespace Mod.DynamicEncounters.Features.Market.Services;

public class RecipePriceCalculator(IServiceProvider provider) : IRecipePriceCalculator
{
    public async Task<Dictionary<string, RecipeOutputData>> GetItemPriceMap()
    {
        var bank = provider.GetGameplayBank();
        var recipes = provider.GetRequiredService<IRecipes>();
        var allRecipes = await recipes.GetAllRecipes();
        var orePriceReader = provider.GetRequiredService<IOrePriceRepository>();
        var priceMap = await orePriceReader.GetOrePrices();
        var recipeOutputMap = new Dictionary<string, RecipeOutputData>();

        var productRecipeMap = new Dictionary<string, RecipeDefinition>();

        foreach (var recipe in allRecipes)
        {
            var product = recipe.products.First();
            var itemName = bank.GetDefinition(product.itemId);
            if (itemName == null) continue;

            productRecipeMap.TryAdd(
                itemName.Name,
                new RecipeDefinition(
                    recipe.id,
                    bank.GetDefinition(product.itemId)!.Name,
                    TimeSpan.FromSeconds(recipe.time),
                    recipe.ingredients.Select(x => new RecipeItem(
                        bank.GetDefinition(x.itemId)!.Name,
                        x.itemId,
                        bank.GetDefinition(x.itemId)!.Is<Material>()
                            ? new MaterialQuantity(x.quantity.value)
                            : new Quantity(x.quantity.value)
                    )),
                    recipe.products.Select(x => new RecipeItem(
                        bank.GetDefinition(x.itemId)!.Name,
                        x.itemId,
                        bank.GetDefinition(x.itemId)!.Is<Material>()
                            ? new MaterialQuantity(x.quantity.value)
                            : new Quantity(x.quantity.value)
                    ))
                )
            );
        }
        
        foreach (var kvp in priceMap)
        {
            recipeOutputMap.TryAdd(
                kvp.Key,
                new RecipeOutputData
                {
                    Quanta = kvp.Value,
                    Quantity = new Quantity(1)
                }
            );
        }

        foreach (var kvp in productRecipeMap)
        {
            var output = CalculateRecipeCostV2(
                priceMap,
                productRecipeMap,
                kvp.Value
            );

            priceMap.TryAdd(kvp.Key, output.GetTotalCost());

            var quantityMap = kvp.Value.Products.ToDictionary(
                k => k.ItemName,
                v => v.Quantity
            );
            recipeOutputMap.TryAdd(kvp.Key,
                new RecipeOutputData { Quanta = output.GetTotalCost(), Quantity = quantityMap[kvp.Key] });
        }

        return recipeOutputMap;
    }

    private static CostCalculationResult CalculateRecipeCostV2(
        Dictionary<string, Quanta> priceMap,
        Dictionary<string, RecipeDefinition> recipeMap,
        RecipeDefinition recipe
    )
    {
        var mainProduct = recipe.Products.First();

        var entries = new List<CostCalculationResult.Entry>();

        foreach (var ingredient in recipe.Ingredients)
        {
            if (recipeMap.TryGetValue(ingredient.ItemName, out var ingredientRecipe))
            {
                var result = CalculateRecipeCostV2(
                    priceMap,
                    recipeMap,
                    ingredientRecipe
                );

                // 100 Ore - 65 Pure - Cost 3000
                result = result.CalculateForOutput(ingredient.ItemName, ingredient.Quantity);

                entries.Add(
                    new CostCalculationResult.Entry
                    {
                        Price = result.GetTotalCost(),
                        Quantity = mainProduct.Quantity,
                        InputItemName = ingredient.ItemName,
                        OutputItemName = mainProduct.ItemName
                    }
                );
            }
            else
            {
                if (priceMap.TryGetValue(ingredient.ItemName, out var ingredientUnitPrice))
                {
                    var ingredientPrice = ingredient.Quantity.GetReadableValue() * ingredientUnitPrice;

                    entries.Add(
                        new CostCalculationResult.Entry
                        {
                            Price = ingredientPrice,
                            Quantity = mainProduct.Quantity,
                            InputItemName = ingredient.ItemName,
                            OutputItemName = mainProduct.ItemName
                        }
                    );
                }
            }
        }

        return new CostCalculationResult
        {
            Entries = entries
        };
    }

    private static Quanta CalculateRecipeCost(
        Dictionary<string, Quanta> priceMap,
        Dictionary<string, RecipeDefinition> recipeMap,
        RecipeDefinition recipe
    )
    {
        var price = 0d;

        foreach (var ingredient in recipe.Ingredients)
        {
            if (priceMap.TryGetValue(ingredient.ItemName, out var itemPrice))
            {
                price += ingredient.Quantity.GetReadableValue() * itemPrice;
                return price;
            }

            if (recipeMap.TryGetValue(ingredient.ItemName, out var ingredientRecipe))
            {
                price += CalculateRecipeCost(
                    priceMap,
                    recipeMap,
                    ingredientRecipe
                );

                priceMap.TryAdd(ingredient.ItemName, price);
            }
        }

        return price;
    }
}