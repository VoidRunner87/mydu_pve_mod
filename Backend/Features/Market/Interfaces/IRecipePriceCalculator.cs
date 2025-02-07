using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Market.Data;

namespace Mod.DynamicEncounters.Features.Market.Interfaces;

public interface IRecipePriceCalculator
{
    Task<Dictionary<string, RecipeOutputData>> GetItemPriceMap();
}