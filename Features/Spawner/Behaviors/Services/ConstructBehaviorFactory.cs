using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Services;

public class ConstructBehaviorFactory : IConstructBehaviorFactory
{
    public IConstructBehavior Create(ulong constructId, IConstructDefinition constructDefinition, string behavior)
    {
        switch (behavior)
        {
            case "aggressive":
                return new AggressiveBehavior(constructId, constructDefinition).WithErrorHandler();
            case "follow-target":
                return new FollowTargetBehaviorV2(constructId, constructDefinition).WithErrorHandler();
            default:
                return new WreckBehavior().WithErrorHandler();
        }
    }
}