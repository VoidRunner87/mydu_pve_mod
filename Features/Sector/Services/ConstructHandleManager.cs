using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotLib.BotClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;

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
    
    public async Task CleanupExpiredConstructHandlesAsync(Client client, Vec3 sector)
    {
        var expirationMinutes = await _featureReaderService
            .GetIntValueAsync(ConstructHandleExpirationMinutesFeatureName, 360);

        var expiredHandles = await _repository.FindExpiredAsync(expirationMinutes, sector);
        var scriptActionFactory = provider.GetRequiredService<IScriptActionFactory>();
        
        var taskList = new List<Task>();

        foreach (var handle in expiredHandles)
        {
            try
            {
                if (!string.IsNullOrEmpty(handle.OnCleanupScript))
                {
                    var scriptAction = scriptActionFactory.Create(
                        new ScriptActionItem
                        {
                            ConstructId = handle.ConstructId,
                            Type = handle.OnCleanupScript
                        }
                    );

                    taskList.Add(scriptAction.ExecuteAsync(
                        new ScriptContext(
                            provider,
                            new HashSet<ulong>(),
                            handle.Sector
                        )
                    ));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete construct {ConstructId} on Orleans. Ignoring it", handle.ConstructId);
            }
            
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

    public async Task CleanupConstructHandlesInSectorAsync(Client client, Vec3 sector)
    {
        var expiredHandles = (await _repository.FindInSectorAsync(sector)).ToList();
        var scriptActionFactory = provider.GetRequiredService<IScriptActionFactory>();
        
        var taskListActionExec = new List<Task>();

        foreach (var handle in expiredHandles)
        {
            try
            {
                if (!string.IsNullOrEmpty(handle.OnCleanupScript))
                {
                    var scriptAction = scriptActionFactory.Create(
                        new ScriptActionItem
                        {
                            ConstructId = handle.ConstructId,
                            Type = handle.OnCleanupScript
                        }
                    );

                    taskListActionExec.Add(scriptAction.ExecuteAsync(
                        new ScriptContext(
                            provider,
                            new HashSet<ulong>(),
                            handle.Sector
                        )
                    ));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete construct {ConstructId} on Orleans. Ignoring it", handle.ConstructId);
            }
            
            _logger.LogInformation("Construct {ConstructId} removed from tracking", handle.ConstructId);
        }

        try
        {
            await Task.WhenAll(taskListActionExec);
            
            // Remove from tracking
            await Task.WhenAll(expiredHandles.Select(handle => _repository.DeleteAsync(handle.Id)));
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
}