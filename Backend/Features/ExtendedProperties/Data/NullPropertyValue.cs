using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Data;

public class NullPropertyValue : IPropertyValue
{
    public object? Value => default;

    public T? ValueAs<T>()
    {
        return default;
    }
}