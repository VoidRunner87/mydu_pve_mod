using System;
using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class SpawnedConstructHandle(
    ulong constructId,
    IConstructDefinition constructDef,
    IEnumerable<IConstructBehavior> behaviors
) : IConstructHandle
{
    public ulong ConstructId { get; set; } = constructId;
    public Guid ConstructDefinitionId => constructDef.Id;
    public IEnumerable<IConstructBehavior> Behaviors { get; set; } = behaviors;

    public ulong GetKey() => ConstructId;
}