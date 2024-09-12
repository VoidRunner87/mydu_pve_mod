using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Repository;
using Mod.DynamicEncounters.Features.Loot.Service;

namespace Mod.DynamicEncounters.Features.Loot;

public static class LootRegistration
{
    public static void RegisterLootSystem(this IServiceCollection services)
    {
        services.AddSingleton<IItemSpawnerService, ItemSpawnerService>();
        services.AddSingleton<ILootDefinitionRepository, LootDefinitionRepository>();
        services.AddSingleton<ILootGeneratorService, LootGeneratorService>();
    }
}