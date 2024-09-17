using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Services;

public class ConstructBehaviorFactory : IConstructBehaviorFactory
{
    public IConstructBehavior Create(ulong constructId, IPrefab prefab, string behavior)
    {
        switch (behavior)
        {
            case "aggressive":
                return new AggressiveBehavior(constructId, prefab).WithErrorHandler();
            case "follow-target":
                return new FollowTargetBehaviorV3(constructId, prefab).WithErrorHandler();
            default:
                return new WreckBehavior().WithErrorHandler();
        }
    }

    public IEnumerable<IConstructBehavior> CreateBehaviors(ulong constructId, IPrefab prefab)
    {
        if (prefab.DefinitionItem.InitialBehaviors.Count == 0)
        {
            return [new WreckBehavior()];
        }
        
        return prefab.DefinitionItem.InitialBehaviors
            .Select(x => Create(constructId, prefab, x));
    }
}