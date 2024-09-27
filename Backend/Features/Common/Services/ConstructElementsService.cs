using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class ConstructElementsService(IServiceProvider provider) : IConstructElementsService
{
    private readonly IClusterClient _orleans = provider.GetOrleans();

    public async Task<IEnumerable<ElementId>> GetPvpRadarElements(ulong constructId)
    {
        return await _orleans.GetConstructElementsGrain(constructId).GetElementsOfType<RadarPVPUnit>();
    }

    public async Task<IEnumerable<ElementId>> GetPvpSeatElements(ulong constructId)
    {
        return await _orleans.GetConstructElementsGrain(constructId).GetElementsOfType<PVPSeatUnit>();
    }

    public async Task<IEnumerable<ElementId>> GetWeaponUnits(ulong constructId)
    {
        return await _orleans.GetConstructElementsGrain(constructId).GetElementsOfType<WeaponUnit>();
    }

    public async Task<ElementInfo> GetElement(ulong constructId, ElementId elementId)
    {
        return await _orleans.GetConstructElementsGrain(constructId).GetElement(elementId);
    }

    public async Task<ElementId> GetCoreUnit(ulong constructId)
    {
        return (await _orleans.GetConstructElementsGrain(constructId).GetElementsOfType<CoreUnit>()).SingleOrDefault();
    }

    public IConstructElementsService NoCache()
    {
        return this;
    }

    public void Invalidate()
    {
    }
}