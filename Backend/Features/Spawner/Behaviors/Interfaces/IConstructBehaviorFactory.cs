using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

public interface IConstructBehaviorFactory
{
    IConstructBehavior Create(
        ulong constructId, 
        IPrefab prefab, 
        string behavior
    );

    IEnumerable<IConstructBehavior> CreateBehaviors(
        ulong constructId,
        IPrefab prefab,
        IEnumerable<string> behaviors
    );
}