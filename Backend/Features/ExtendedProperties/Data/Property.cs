using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Data;

public class Property(TraitPropertyId id, IPropertyValue value) : IProperty
{
    public TraitPropertyId Id { get; } = id;
    public IPropertyValue Prop { get; } = value;
}