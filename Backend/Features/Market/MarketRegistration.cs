using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Market.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;
using Mod.DynamicEncounters.Features.Market.Repository;
using Mod.DynamicEncounters.Features.Market.Services;

namespace Mod.DynamicEncounters.Features.Market;

public static class MarketRegistration
{
    public static void RegisterMarketServices(this IServiceCollection services)
    {
        services.AddSingleton<IMarketOrderRepository, MarketOrderRepository>();
        services.AddSingleton<IOrePriceRepository>(p => new CachedOrePriceRepository(new OrePriceRepository(p)));
        services.AddSingleton<IRecipePriceCalculator>(p =>
            new CachedRecipePriceCalculator(new RecipePriceCalculator(p)));
    }
}