using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class WreckBehavior : IConstructBehavior
{
    public bool IsActive()
    {
        return false;
    }

    public BehaviorTaskCategory Category => BehaviorTaskCategory.LowPriority;

    public Task InitializeAsync(BehaviorContext context)
    {
        context.IsActiveWreck = true;
        context.IsAlive = false;
        return Task.CompletedTask;
    }

    public Task TickAsync(BehaviorContext context)
    {
        context.IsActiveWreck = true; //TODO
        return Task.CompletedTask;
    }
}