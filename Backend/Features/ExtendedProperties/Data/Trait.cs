using System.Collections.Generic;
using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Data;

public class Trait(TraitId id, string name, string description, Dictionary<string, IProperty> properties)
    : ITrait
{
    public TraitId Id { get; } = id;
    public string Name { get; } = name;
    public string Description { get; } = description;
    public Dictionary<string, IProperty> Properties { get; } = properties;
}