using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class AlienCoreRotationLoop : ModBase
{
    public override async Task Loop()
    {
        var provider = ServiceProvider;
        var logger = provider.CreateLogger<SectorLoop>();

        var featureService = provider.GetRequiredService<IFeatureReaderService>();
        
        try
        {
            while (true)
            {
                await Task.Delay(3000);
                var isEnabled = await featureService.GetEnabledValue<SectorLoop>(false);

                if (isEnabled)
                {
                    await ExecuteAction();
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to execute {Name}", nameof(AlienCoreRotationLoop));
            // TODO implement alerting on too many failures
        }
    }

    private async Task ExecuteAction()
    {
        await Task.Yield();
    }
}