using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;

namespace Mod.DynamicEncounters.Features.Market.Interfaces;

public interface IOrePriceRepository
{
    Task<Dictionary<string, Quanta>> GetOrePrices();
}