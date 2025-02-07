using System;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Common.Interfaces;
using Orleans;

namespace Mod.DynamicEncounters.Helpers;

public static class ServiceProviderHelpers
{
    public static ILogger<T> CreateLogger<T>(this IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<T>();

    public static IClusterClient GetOrleans(this IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<IClusterClient>();

    public static IGameplayBank GetGameplayBank(this IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<IGameplayBank>();
    
    public static IRandomProvider GetRandomProvider(this IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<IRandomProvider>();
}