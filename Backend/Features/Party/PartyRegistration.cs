using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Party.Interfaces;
using Mod.DynamicEncounters.Features.Party.Repository;
using Mod.DynamicEncounters.Features.Party.Services;

namespace Mod.DynamicEncounters.Features.Party;

public static class PartyRegistration
{
    public static void RegisterPlayerParty(this IServiceCollection services)
    {
        services.AddSingleton<IPlayerPartyService, PlayerPartyService>();
        services.AddSingleton<IPlayerPartyRepository, PlayerPartyRepository>();
    }
}