using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class ExpirationNamesLoop : ModBase
{
    public override async Task Loop()
    {
        while (true)
        {
            var logger = ServiceProvider.CreateLogger<SectorLoop>();

            try
            {
                var featureService = ServiceProvider.GetRequiredService<IFeatureReaderService>();
                var sectorPoolManager = ServiceProvider.GetRequiredService<ISectorPoolManager>();

                if (await featureService.GetEnabledValue<SectorLoop>(false))
                {
                    await sectorPoolManager.UpdateExpirationNames();
                }

                await Task.Delay(TimeSpan.FromSeconds(30));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to UpdateExpirationNames");
                
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }
    }
}