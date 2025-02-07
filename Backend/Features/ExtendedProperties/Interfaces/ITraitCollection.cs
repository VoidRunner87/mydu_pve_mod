using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

public interface ITraitCollection
{
    ITrait? FindAsync(string name);
    Dictionary<string, ITrait> Map();
}