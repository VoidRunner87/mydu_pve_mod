using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Data;

public class OverridePropertyValue(
    IPropertyValue traitPropVal,
    IPropertyValue propVal
) : IPropertyValue
{
    public object? Value => propVal.As<object>() ?? traitPropVal.As<object>();

    public T? As<T>()
    {
        return (T?)Value;
    }
}