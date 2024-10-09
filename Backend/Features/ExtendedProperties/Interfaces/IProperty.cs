using Mod.DynamicEncounters.Features.ExtendedProperties.Data;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

public interface IProperty
{
    TraitPropertyId Id { get; }
    IPropertyValue Prop { get; }
}