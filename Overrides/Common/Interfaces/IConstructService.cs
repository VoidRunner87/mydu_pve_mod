using System.Threading.Tasks;
using Mod.DynamicEncounters.Overrides.Common.Data;

namespace Mod.DynamicEncounters.Overrides.Common.Interfaces;

public interface IConstructService
{
    Task<ConstructItem?> GetConstructInfoCached(ulong constructId);
    Task<double> GetCoreStressRatioCached(ulong constructId);
}