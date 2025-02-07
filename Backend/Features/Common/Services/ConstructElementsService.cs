using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
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

    public async Task<Dictionary<string, List<WeaponEffectivenessData>>> GetDamagingWeaponsEffectiveness(ulong constructId)
    {
        var bank = provider.GetGameplayBank();
        
        var weaponUnits = await GetWeaponUnits(constructId); // TODO still counts stasis
        var weaponInfosTask = weaponUnits.Select(x => GetElement(constructId, x));

        var weaponInfos = await Task.WhenAll(weaponInfosTask);

        var result = new Dictionary<string, List<WeaponEffectivenessData>>();
        
        foreach (var elementInfo in weaponInfos)
        {
            var definition = bank.GetDefinition(elementInfo.elementType);
            if (definition?.BaseObject is not WeaponUnit weaponUnit) continue;
            if (weaponUnit.baseDamage <= 0) continue;
            
            var item = new WeaponEffectivenessData
            {
                Name = definition.Name,
                HitPointsRatio = 1
            };
            
            if (!elementInfo.properties.TryGetValue("hitpointsRatio", out var propValue))
            {
                if (!result.TryAdd(item.Name, [item]))
                {
                    result[item.Name].Add(item);
                }
            }

            if (propValue != null)
            {
               item.HitPointsRatio = propValue.doubleValue;
               
               if (!result.TryAdd(item.Name, [item]))
               {
                   result[item.Name].Add(item);
               }
            }
        }

        return result;
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