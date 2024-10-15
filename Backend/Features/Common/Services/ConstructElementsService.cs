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

    public async Task<IEnumerable<ElementId>> GetContainerElements(ulong constructId)
    {
        return await _orleans.GetConstructElementsGrain(constructId).GetElementsOfType<ContainerUnit>();
    }

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

    public async Task<IEnumerable<ElementId>> GetSpaceEngineUnits(ulong constructId)
    {
        return await _orleans.GetConstructElementsGrain(constructId).GetElementsOfType<SpaceEngine>();
    }
    
    public async Task<double> GetAllSpaceEnginesPower(ulong constructId)
    {
        var engines = await GetSpaceEngineUnits(constructId);
        var engineInfosTask = engines.Select(x => GetElement(constructId, x));

        var engineInfo = await Task.WhenAll(engineInfosTask);

        var hitPoints = new List<double>();
        
        foreach (var elementInfo in engineInfo)
        {
            if (!elementInfo.properties.TryGetValue("hitpointsRatio", out var propValue))
            {
                hitPoints.Add(1);
            }

            if (propValue != null)
            {
                hitPoints.Add(propValue.doubleValue);
            }
        }

        if (hitPoints.Count == 0)
        {
            return 0;
        }

        double brokenCount = hitPoints.Count(x => x <= 0.01d);
        var functionalCount = hitPoints.Count - brokenCount;

        return functionalCount / hitPoints.Count;
    }

    public async Task<int> GetFunctionalDamageWeaponCount(ulong constructId)
    {
        var weaponUnits = await GetWeaponUnits(constructId);
        var weaponInfosTask = weaponUnits.Select(x => GetElement(constructId, x));

        var weaponInfos = await Task.WhenAll(weaponInfosTask);

        var hitPoints = new List<double>();
        
        foreach (var elementInfo in weaponInfos)
        {
            if (!elementInfo.properties.TryGetValue("hitpointsRatio", out var propValue))
            {
                hitPoints.Add(1);
            }

            if (propValue != null)
            {
                hitPoints.Add(propValue.doubleValue);
            }
        }

        if (hitPoints.Count == 0)
        {
            return 0;
        }

        var brokenCount = hitPoints.Count(x => x <= 0.01d);
        var functionalCount = hitPoints.Count - brokenCount;

        return functionalCount;
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