using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class SectorActivationWorker : BackgroundService
{
    private readonly IServiceProvider _provider = ModBase.ServiceProvider;
    
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
                ModBase.ServiceProvider.CreateLogger<SectorActivationWorker>()
                    .LogError(e, "{Type} Exception: {Message}", GetType().Name, e.Message);
            }
        }
    }
    
    private async Task Tick(CancellationToken stoppingToken)
    {
        var logger = ModBase.ServiceProvider.CreateLogger<SectorActivationWorker>();

        try
        {
            await ExecuteAction(stoppingToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to execute {Name}", nameof(SectorActivationWorker));
        }
    }
    
    private async Task ExecuteAction(CancellationToken stoppingToken)
    {
        var sw = new Stopwatch();
        sw.Start();

        var logger = _provider.CreateLogger<SectorActivationWorker>();

        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            { "Thread", Environment.CurrentManagedThreadId }
        });

        var sectorPoolManager = _provider.GetRequiredService<ISectorPoolManager>();
        await sectorPoolManager.ActivateEnteredSectors().WaitAsync(stoppingToken);

        logger.LogInformation("SectorActivationWorker took {Time}ms", sw.ElapsedMilliseconds);
        StatsRecorder.Record(nameof(SectorActivationWorker), sw.ElapsedMilliseconds);
    }
}