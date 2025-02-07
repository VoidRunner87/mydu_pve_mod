using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Market.Data;

namespace Mod.DynamicEncounters.Features.Market.Interfaces;

public interface IMarketOrderRepository
{
    Task<double> GetAveragePriceOfItemAsync(ulong itemTypeId);
    Task CreateMarketOrder(MarketItem marketItem);
}