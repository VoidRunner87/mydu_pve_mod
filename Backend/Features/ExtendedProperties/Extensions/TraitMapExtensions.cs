using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Extensions;

public static class TraitMapExtensions
{
    public static bool TryGetPropertyValue<T>(this ITrait trait, string propertyName, out T value, T defaultValue = default)
    {
        if (trait.Properties.TryGetValue(propertyName, out var prop))
        {
            var stringValue = prop.Prop.ValueAs<string>();
            var jTokenVal = JToken.FromObject(stringValue);

            value = jTokenVal.Value<T>();
            return true;
        }

        value = defaultValue;
        return false;
    }
}