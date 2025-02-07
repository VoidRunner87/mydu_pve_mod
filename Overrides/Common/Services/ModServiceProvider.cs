using System;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Overrides.Common.Interfaces;

namespace Mod.DynamicEncounters.Overrides.Common.Services;

// ReSharper disable once InconsistentNaming
public static class ModServiceProvider
{
    private static IServiceProvider External { get; set; }
    private static IServiceProvider Internal { get; set; }
    
    public static void Initialize(IServiceProvider provider)
    {
        External = provider;
        
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IMyDuInjectionService, MyDuInjectionService>();
        serviceCollection.AddSingleton<ICachedConstructDataService, CachedConstructDataService>();

        Internal = serviceCollection.BuildServiceProvider();
    }

    public static T Get<T>() => Internal.GetRequiredService<T>();
    public static T GetExternal<T>() => External.GetRequiredService<T>();
}