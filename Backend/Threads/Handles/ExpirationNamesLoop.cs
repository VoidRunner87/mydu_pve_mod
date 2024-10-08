using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class ExpirationNamesLoop(IThreadManager tm, CancellationToken ct) :
    ThreadHandle(ThreadId.ExpirationNames, tm, ct)
{
    public override async Task Tick()
    {
        var logger = ModBase.ServiceProvider.CreateLogger<SectorLoop>();

        try
        {
            var featureService = ModBase.ServiceProvider.GetRequiredService<IFeatureReaderService>();
            var sectorPoolManager = ModBase.ServiceProvider.GetRequiredService<ISectorPoolManager>();

            if (await featureService.GetEnabledValue<SectorLoop>(false))
            {
                await sectorPoolManager.UpdateExpirationNames();
            }

            Thread.Sleep(TimeSpan.FromSeconds(30));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to UpdateExpirationNames");

            Thread.Sleep(TimeSpan.FromSeconds(30));
        }
    }
}