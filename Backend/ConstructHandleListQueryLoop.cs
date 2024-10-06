using System;
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
                    ConstructBehaviorLoop.ConstructHandles.Clear();
                    foreach (var item in items)
                    {
                        var added = ConstructBehaviorLoop.ConstructHandles.TryAdd(item.ConstructId, item);
                        if (!added)
                        {
                            logger.LogError("Failed to Add {Construct}", item.ConstructId);
                        }
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