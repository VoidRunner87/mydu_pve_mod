namespace Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

public interface IPropertyValue
{
    object? Value { get; }
    T? ValueAs<T>();
}