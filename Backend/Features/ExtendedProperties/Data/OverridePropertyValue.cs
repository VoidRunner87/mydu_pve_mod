using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Data;

public class OverridePropertyValue(
    IPropertyValue traitPropVal,
    IPropertyValue propVal
) : IPropertyValue
{
    public object? Value => propVal.ValueAs<object>() ?? traitPropVal.ValueAs<object>();

    public T? ValueAs<T>()
    {
        return (T?)Value;
    }
}