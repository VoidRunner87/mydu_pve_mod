using System.Collections.Generic;
using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Data;

public class TraitCollection(Dictionary<string, ITrait> map) : ITraitCollection
{
    public ITrait? FindAsync(string name)
    {
        return !map.TryGetValue(name, out var trait) ? null : trait;
    }

    public Dictionary<string, ITrait> Map() => map;
}