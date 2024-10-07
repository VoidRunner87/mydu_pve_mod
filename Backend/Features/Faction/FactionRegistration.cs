using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Faction.Repository;

namespace Mod.DynamicEncounters.Features.Faction;

public static class FactionRegistration
{
    public static void RegisterFaction(this IServiceCollection services)
    {
        services.AddSingleton<IFactionRepository, FactionRepository>();
        services.AddSingleton<IFactionTerritoryRepository, FactionTerritoryRepository>();
    }
}