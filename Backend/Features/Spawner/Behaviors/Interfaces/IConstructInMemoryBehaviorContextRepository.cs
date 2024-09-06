using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

public interface IConstructInMemoryBehaviorContextRepository
{
    BehaviorContext GetOrDefault(ulong constructId, BehaviorContext defaultValue);
    void Set(ulong constructId, BehaviorContext context);

    void Cleanup();
}