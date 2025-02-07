using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Mod.DynamicEncounters.Stubs;

public static class StubHostingExtensions
{
    public static async Task StartServicesV2(
        this IServiceProvider provider,
        CancellationToken externalToken = default)
    {
        Console.WriteLine(nameof(StartServicesV2));
        Log.Information(nameof(StartServicesV2));
        
        var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        cts.CancelAfter(TimeSpan.FromSeconds(600.0));
        foreach (var service in provider.GetServices<IHostedService>())
        {
            Console.WriteLine($"Starting {service.GetType()}");
            Log.Information($"Starting {service.GetType()}");
            
            await service.StartAsync(cts.Token);
        }
    }
}