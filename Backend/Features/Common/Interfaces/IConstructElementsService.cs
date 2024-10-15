using System.Collections.Generic;
using System.Threading.Tasks;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IConstructElementsService
{
    Task<IEnumerable<ElementId>> GetContainerElements(ulong constructId);
    Task<IEnumerable<ElementId>> GetPvpRadarElements(ulong constructId);
    Task<IEnumerable<ElementId>> GetPvpSeatElements(ulong constructId);
    Task<IEnumerable<ElementId>> GetWeaponUnits(ulong constructId);
    Task<IEnumerable<ElementId>> GetSpaceEngineUnits(ulong constructId);
    Task<double> GetAllSpaceEnginesPower(ulong constructId);
    Task<int> GetFunctionalDamageWeaponCount(ulong constructId);
    Task<ElementInfo> GetElement(ulong constructId, ElementId elementId);
    Task<ElementId> GetCoreUnit(ulong constructId);
    IConstructElementsService NoCache();
}