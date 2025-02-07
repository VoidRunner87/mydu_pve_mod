using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Warp.Interfaces;
using Mod.DynamicEncounters.Features.Warp.Services;

namespace Mod.DynamicEncounters.Features.Warp;

public static class WarpServicesRegistration
{
    public static void RegisterWarpServices(this IServiceCollection services)
    {
        services.AddSingleton<IWarpAnchorService, WarpAnchorService>();
    }
}