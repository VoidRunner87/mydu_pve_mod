using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class NullBehavior : IConstructBehavior
{
    public BehaviorTaskCategory Category => BehaviorTaskCategory.MediumPriority;

    public Task InitializeAsync(BehaviorContext context)
    {
        return Task.CompletedTask;
    }

    public async Task TickAsync(BehaviorContext context)
    {
        await Task.Yield();
    }
}