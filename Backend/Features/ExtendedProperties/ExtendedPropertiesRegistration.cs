using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;
using Mod.DynamicEncounters.Features.ExtendedProperties.Repository;

namespace Mod.DynamicEncounters.Features.ExtendedProperties;

public static class ExtendedPropertiesRegistration
{
    public static void RegisterExtendedProperties(this IServiceCollection services)
    {
        services.AddSingleton<ITraitRepository>(p => new CachedTraitRepository(new TraitRepository(p)));
    }
}