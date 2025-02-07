using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Data;

public class PropertyValue(object? value) : IPropertyValue
{
    public object? Value { get; } = value;

    public T? ValueAs<T>()
    {
        return (T)Value;
    }
}