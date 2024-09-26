using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IConstructElementsService : IExpireCache
{
    Task<IEnumerable<ElementId>> GetPvpRadarElements(ulong constructId);
    Task<IEnumerable<ElementId>> GetPvpSeatElements(ulong constructId);
    Task<IEnumerable<ElementId>> GetWeaponUnits(ulong constructId);
    Task<ElementInfo> GetElement(ulong constructId, ElementId elementId);

    Task<ElementId> GetCoreUnit(ulong constructId);
}