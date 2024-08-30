using System;
using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IConstructHandle : IHasKey<ulong>
{
    ulong ConstructId { get; }
    Guid ConstructDefinitionId { get; }
    IEnumerable<IConstructBehavior> Behaviors { get; }
}