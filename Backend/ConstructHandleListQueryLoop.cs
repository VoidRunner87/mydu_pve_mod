using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class ConstructHandleListQueryLoop : ModBase
{
    public override async Task Loop()
    {
        while (true)
        {
            try
            {
                var logger = ServiceProvider.CreateLogger<ConstructHandleListQueryLoop>();
                
                var constructHandleRepository = ServiceProvider.GetRequiredService<IConstructHandleRepository>();

                var items = await constructHandleRepository.FindActiveHandlesAsync();

                lock (ConstructBehaviorLoop.ListLock)
                {
                    // ConstructBehaviorLoop.ConstructHandles.Clear();
                    foreach (var item in items)
                    {
                        ConstructBehaviorLoop.ConstructHandles.TryAdd(item.ConstructId, item);
                    }

                    var deadConstructHandles = ConstructBehaviorLoop.ConstructHandleHeartbeat
                        .Where(x => DateTime.UtcNow - x.Value > TimeSpan.FromMinutes(30));

                    foreach (var kvp in deadConstructHandles)
                    {
                        ConstructBehaviorLoop.ConstructHandles.TryRemove(kvp.Key, out _);
                        logger.LogWarning("Removed Construct Handle {Construct} that failed to be removed", kvp.Value);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
                
                RecordHeartBeat();
            }
            catch (Exception e)
            {
                var logger = ServiceProvider.CreateLogger<ConstructHandleListQueryLoop>();
                logger.LogError(e, "Failed to Query Construct Handles");
            }
        }
    }
}