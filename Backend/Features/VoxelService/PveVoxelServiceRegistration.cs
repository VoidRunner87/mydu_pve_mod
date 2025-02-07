using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.VoxelService.Interfaces;
using Mod.DynamicEncounters.Features.VoxelService.Services;

namespace Mod.DynamicEncounters.Features.VoxelService;

public static class PveVoxelServiceRegistration
{
    public static void RegisterVoxelService(this IServiceCollection services)
    {
        services.AddSingleton<IVoxelServiceClient, VoxelServiceClient>();
    }
}