using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IConstructDefinitionFactory
{
    IPrefab Create(PrefabItem definitionItem);
}