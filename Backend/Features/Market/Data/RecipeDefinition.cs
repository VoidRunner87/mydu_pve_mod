using System;
using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Market.Data;

public class RecipeDefinition(
    ulong id,
    ElementTypeName elementTypeName,
    TimeSpan time,
    IEnumerable<RecipeItem> ingredients,
    IEnumerable<RecipeItem> products
)
{
    public ulong Id { get; } = id;
    public ElementTypeName ElementTypeName { get; } = elementTypeName;
    public TimeSpan Time { get; } = time;
    public IEnumerable<RecipeItem> Ingredients { get; } = ingredients;
    public IEnumerable<RecipeItem> Products { get; } = products;
}