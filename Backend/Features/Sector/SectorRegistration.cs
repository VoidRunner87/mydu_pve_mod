using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Repository;
using Mod.DynamicEncounters.Features.Sector.Services;

namespace Mod.DynamicEncounters.Features.Sector;

public static class SectorRegistration
{
    public static void RegisterSectorGeneration(this IServiceCollection services)
    {
        services.AddSingleton<ISectorPoolManager, SectorPoolManager>();
        services.AddSingleton<ISectorInstanceRepository, SectorInstanceRepository>();
        services.AddSingleton<ISectorEncounterRepository, SectorEncounterRepository>();
        services.AddSingleton<IConstructHandleManager, ConstructHandleManager>();
    }
}