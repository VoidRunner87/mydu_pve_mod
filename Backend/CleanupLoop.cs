using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Services;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class CleanupLoop(TimeSpan loopTimer) : ModBase
{
    private IConstructService _constructService;
    private ILogger<CleanupLoop> _logger;
    private IConstructHandleRepository _constructHandleRepository;

    public override Task Start()
    {
        _constructService = ServiceProvider.GetRequiredService<IConstructService>();
        _constructHandleRepository = ServiceProvider.GetRequiredService<IConstructHandleRepository>();
        _logger = ServiceProvider.CreateLogger<CleanupLoop>();
        
        var taskCompletionSource = new TaskCompletionSource();
        
        var timer = new Timer(loopTimer);
        timer.Elapsed += async (_,_) => await OnTimer();
        timer.Start();

        return taskCompletionSource.Task;
    }

    private async Task OnTimer()
    {
        while (!ConstructsPendingDelete.Data.IsEmpty)
        {
            if (!ConstructsPendingDelete.Data.TryPeek(out var constructId))
            {
                continue;
            }
            
            try
            {
                await _constructService.DeleteAsync(constructId);
                await _constructHandleRepository.DeleteAsync(constructId);
                await Task.Delay(500);
                
                ConstructsPendingDelete.Data.TryDequeue(out _);
                
                _logger.LogInformation("Cleaned up {Construct}", constructId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to Cleanup {Construct}", constructId);
            }
        }
    }
}