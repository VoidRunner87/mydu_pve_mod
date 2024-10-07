using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class ConstructHandleListQueryLoop(IThreadManager tm, CancellationToken ct) :
    ThreadHandle(ThreadId.ConstructHandleQuery, tm, ct)
{
    public override async Task Tick()
    {
        try
        {
            var logger = ModBase.ServiceProvider.CreateLogger<ConstructHandleListQueryLoop>();

            var constructHandleRepository = ModBase.ServiceProvider.GetRequiredService<IConstructHandleRepository>();

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
                    ConstructBehaviorLoop.ConstructHandleHeartbeat.TryRemove(kvp.Key, out _);
                    logger.LogWarning("Removed Construct Handle {Construct} that failed to be removed", kvp.Key);
                }
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));

            ReportHeartbeat();
        }
        catch (Exception e)
        {
            var logger = ModBase.ServiceProvider.CreateLogger<ConstructHandleListQueryLoop>();
            logger.LogError(e, "Failed to Query Construct Handles");
        }
    }
}