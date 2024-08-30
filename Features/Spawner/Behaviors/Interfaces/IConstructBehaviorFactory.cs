using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

public interface IConstructBehaviorFactory
{
    IConstructBehavior Create(
        ulong constructId, 
        IConstructDefinition constructDefinition, 
        string behavior
    );
}