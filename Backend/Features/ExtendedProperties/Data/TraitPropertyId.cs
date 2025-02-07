using System;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Data;

public readonly struct TraitPropertyId(TraitId traitId, Guid id)
{
    public TraitId TraitId { get; } = traitId;
    public Guid Id { get; } = id;
}