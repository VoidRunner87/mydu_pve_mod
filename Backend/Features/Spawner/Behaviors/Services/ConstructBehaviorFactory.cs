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
            case "alive":
                return new AliveCheckBehavior(constructId, prefab).WithErrorHandler();
            case "select-target":
                return new SelectTargetBehavior(constructId, prefab).WithErrorHandler();
            case "aggressive":
                return new AggressiveBehavior(constructId, prefab).WithErrorHandler();
            case "follow-target":
                return new FollowTargetBehaviorV2(constructId, prefab).WithErrorHandler();
            default:
                return new WreckBehavior().WithErrorHandler();
        }
    }

    public IEnumerable<IConstructBehavior> CreateBehaviors(
        ulong constructId, 
        IPrefab prefab,
        IEnumerable<string> behaviors
    )
    {
        if (prefab.DefinitionItem.InitialBehaviors.Count == 0)
        {
            return [new WreckBehavior()];
        }

        var behaviorList = behaviors.ToList();
        var finalBehaviors = new List<string>();
        
        // for compatibility
        if (!behaviorList.Any())
        {
            finalBehaviors.Add("alive");
            finalBehaviors.Add("select-target");
            finalBehaviors.AddRange(prefab.DefinitionItem.InitialBehaviors);
        }
        
        finalBehaviors.AddRange(behaviorList);
        
        return finalBehaviors.Select(x => Create(constructId, prefab, x));
    }
}