using System;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IConstructDefinition : IHasKey<string>
{
    Guid Id { get; }
    PrefabItem DefinitionItem { get; }
    IConstructEvents Events { get; }
}