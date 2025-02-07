using Mod.DynamicEncounters.Overrides.Common.Data;

namespace Mod.DynamicEncounters.Overrides.Common.Interfaces;

public interface ICachedConstructDataService
{
    ConstructData? Get(ulong constructId);
    void Set(ulong constructId, ConstructData data);
}