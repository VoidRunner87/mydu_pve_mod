using System;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Data;

public readonly struct TraitId(Guid id)
{
    public Guid Id { get; } = id;
}