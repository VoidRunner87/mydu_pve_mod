using System.Collections.Generic;
using Mod.DynamicEncounters.Features.ExtendedProperties.Data;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

public interface ITrait
{
    TraitId Id { get; }
    string Name { get; }
    string Description { get; }
    Dictionary<string, IProperty> Properties { get; }
}