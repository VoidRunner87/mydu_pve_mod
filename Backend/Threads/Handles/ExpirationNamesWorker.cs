using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class ExpirationNamesWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);
            
                await Tick(cts.Token);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (Exception e)
            {
                ModBase.ServiceProvider.CreateLogger<ExpirationNamesWorker>()
                    .LogError(e, "{Type} Exception: {Message}", GetType().Name, e.Message);
            }
        }
    }

    private static async Task Tick(CancellationToken stoppingToken)
    {
        var logger = ModBase.ServiceProvider.CreateLogger<ExpirationNamesWorker>();

        try
        {
            var sectorPoolManager = ModBase.ServiceProvider.GetRequiredService<ISectorPoolManager>();
            await sectorPoolManager.UpdateExpirationNames().WaitAsync(stoppingToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to UpdateExpirationNames");
        }
    }
}