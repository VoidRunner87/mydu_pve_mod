using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

public interface IConstructInMemoryBehaviorContextRepository
{
    bool TryGetValue(ulong constructId, out BehaviorContext? context);
    void Set(ulong constructId, BehaviorContext context);

    void Cleanup();
}