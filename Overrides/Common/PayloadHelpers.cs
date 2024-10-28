using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Overrides.Common;

public static class PayloadHelpers
{
    public static T PayloadAs<T>(this ModAction action)
    {
        return JsonConvert.DeserializeObject<T>(action.payload);
    }
}