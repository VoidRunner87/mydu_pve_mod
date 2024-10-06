using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class ConstructBehaviorFeatureCheckLoop : ModBase
{
    public override async Task Loop()
    {
        while (true)
        {
            try
            {
                var featureService = ServiceProvider.GetRequiredService<IFeatureReaderService>();
                ConstructBehaviorLoop.FeatureEnabled =
                    await featureService.GetEnabledValue<ConstructBehaviorLoop>(false);

                await Task.Delay(TimeSpan.FromSeconds(10));
                
                RecordHeartBeat();
            }
            catch (Exception e)
            {
                var logger = ServiceProvider.CreateLogger<ConstructBehaviorFeatureCheckLoop>();

                logger.LogError(e, "Failed to check Feature Status");
            }
        }
    }
}