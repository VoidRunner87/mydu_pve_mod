using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using Mod.DynamicEncounters.Features.NQ.Services;

namespace Mod.DynamicEncounters.Features.NQ;

public static class NQRegistration
{
    public static void RegisterNQServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlayerService, PlayerService>();
        services.AddSingleton<IWalletService, WalletService>();
    }
}