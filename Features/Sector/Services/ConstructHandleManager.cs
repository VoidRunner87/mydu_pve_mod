using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ.Interfaces;
using Orleans;

namespace Mod.DynamicEncounters.Features.Sector.Services;

public class ConstructHandleManager(IServiceProvider provider) : IConstructHandleManager
{
    private const string ConstructHandleExpirationMinutesFeatureName = "ConstructHandleExpirationMinutes";
    
    private readonly IFeatureReaderService _featureReaderService =
        provider.GetRequiredService<IFeatureReaderService>();
    private readonly IConstructHandleRepository _repository =
        provider.GetRequiredService<IConstructHandleRepository>();

    private readonly ILogger<ConstructHandleManager> _logger =
        provider.CreateLogger<ConstructHandleManager>();
    
    public async Task CleanupExpiredConstructHandlesAsync()
    {
        var expirationMinutes = await _featureReaderService
            .GetIntValueAsync(ConstructHandleExpirationMinutesFeatureName, 360);

        var expiredHandles = await _repository.FindExpiredAsync(expirationMinutes);

        var orleans = provider.GetOrleans();

        var taskList = new List<Task>();

        foreach (var handle in expiredHandles)
        {
            await RemoveConstructAsync(orleans, handle, taskList);
            
            // Remove from tracking
            taskList.Add(_repository.DeleteAsync(handle.Id));
            
            _logger.LogInformation("Construct {ConstructId} removed from tracking", handle.ConstructId);
        }

        try
        {
            await Task.WhenAll(taskList);
        }
        catch (AggregateException ae)
        {
            foreach (var exception in ae.InnerExceptions)
            {
                _logger.LogError(exception, "Failed to perform Cleanup");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to perform a series of cleanups on Construct Handle");
        }
        
    }

    private async Task RemoveConstructAsync(IClusterClient orleans, ConstructHandleItem handle, List<Task> taskList)
    {
        try
        {
            var constructInfoGrain = orleans.GetConstructInfoGrain(handle.ConstructId);
            var constructInfo = await constructInfoGrain.Get();
            var ownerId = constructInfo.mutableData.ownerId.playerId;

            // Avoids deleting constructs that are tracked if someone captures it
            // TODO #limitation Remove this hardcoded in favor of configuration
            if (ownerId != 2 && ownerId != 4)
            {
                var task = orleans.GetConstructParentingGrain()
                    .DeleteConstruct(
                        handle.ConstructId,
                        hardDelete: true
                    );

                taskList.Add(task);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete construct {ConstructId} on Orleans", handle.ConstructId);            
        }
    }
}