using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class ConstructBehaviorFeatureCheckLoop(IThreadManager tm, CancellationToken ct) :
    ThreadHandle(ThreadId.BehaviorFeatureCheck, tm, ct)
{
    public override async Task Tick()
    {
        try
        {
            var featureService = ModBase.ServiceProvider.GetRequiredService<IFeatureReaderService>();
            ConstructBehaviorLoop.FeatureEnabled =
                await featureService.GetEnabledValue<ConstructBehaviorLoop>(false);

            Thread.Sleep(TimeSpan.FromSeconds(10));

            ReportHeartbeat();
        }
        catch (Exception e)
        {
            var logger = ModBase.ServiceProvider.CreateLogger<ConstructBehaviorFeatureCheckLoop>();

            logger.LogError(e, "Failed to check Feature Status");
        }
    }
}