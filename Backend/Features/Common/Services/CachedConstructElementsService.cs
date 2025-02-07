using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class CachedConstructElementsService(
    IConstructElementsService service, 
    TimeSpan expirationTimeSpan,
    TimeSpan coreUnitCacheTimeSpan,
    TimeSpan powerCheckTimeSpan
)
    : IConstructElementsService
{
    private readonly TemporaryMemoryCache<ulong, IEnumerable<ElementId>> _containers = new(nameof(_containers), expirationTimeSpan);
    private readonly TemporaryMemoryCache<ulong, IEnumerable<ElementId>> _pvpRadarUnits = new(nameof(_pvpRadarUnits), expirationTimeSpan);
    private readonly TemporaryMemoryCache<ulong, IEnumerable<ElementId>> _weaponUnits = new(nameof(_weaponUnits), expirationTimeSpan);
    private readonly TemporaryMemoryCache<ulong, IEnumerable<ElementId>> _pvpSeatUnits = new(nameof(_pvpSeatUnits), expirationTimeSpan);
    private readonly TemporaryMemoryCache<ulong, IEnumerable<ElementId>> _spaceEngineUnits = new(nameof(_spaceEngineUnits), powerCheckTimeSpan);
    private readonly TemporaryMemoryCache<ulong, double> _spaceEnginePowers = new(nameof(_spaceEnginePowers), powerCheckTimeSpan);
    private readonly TemporaryMemoryCache<ulong, Dictionary<string, List<WeaponEffectivenessData>>> _weaponEffectiveness = 
        new(nameof(_weaponEffectiveness), powerCheckTimeSpan);
    private readonly TemporaryMemoryCache<ulong, ElementId> _coreUnits = new(nameof(_coreUnits), coreUnitCacheTimeSpan);
    private readonly TemporaryMemoryCache<ulong, ElementInfo> _elementInfos = new(nameof(_elementInfos), expirationTimeSpan);

    public Task<IEnumerable<ElementId>> GetContainerElements(ulong constructId)
    {
        return _containers.TryGetOrSetValue(
            constructId,
            () => service.GetContainerElements(constructId),
            ids => !ids.Any()
        );
    }

    public Task<IEnumerable<ElementId>> GetPvpRadarElements(ulong constructId)
    {
        return _pvpRadarUnits.TryGetOrSetValue(
            constructId,
            () => service.GetPvpRadarElements(constructId),
            ids => !ids.Any()
        );
    }

    public Task<IEnumerable<ElementId>> GetPvpSeatElements(ulong constructId)
    {
        return _pvpSeatUnits.TryGetOrSetValue(
            constructId,
            () => service.GetPvpSeatElements(constructId),
            ids => !ids.Any()
        );
    }

    public Task<IEnumerable<ElementId>> GetWeaponUnits(ulong constructId)
    {
        return _weaponUnits.TryGetOrSetValue(
            constructId,
            () => service.GetWeaponUnits(constructId),
            ids => !ids.Any()
        );
    }

    public Task<IEnumerable<ElementId>> GetSpaceEngineUnits(ulong constructId)
    {
        return _spaceEngineUnits.TryGetOrSetValue(
            constructId,
            () => service.GetSpaceEngineUnits(constructId),
            ids => !ids.Any()
        );
    }

    public Task<double> GetAllSpaceEnginesPower(ulong constructId)
    {
        return _spaceEnginePowers.TryGetOrSetValue(
            constructId,
            () => service.GetAllSpaceEnginesPower(constructId),
            val => val <= 0
        );
    }

    public Task<Dictionary<string, List<WeaponEffectivenessData>>> GetDamagingWeaponsEffectiveness(ulong constructId)
    {
        return _weaponEffectiveness.TryGetOrSetValue(
            constructId,
            () => service.GetDamagingWeaponsEffectiveness(constructId)
        );
    }

    public Task<ElementInfo> GetElement(ulong constructId, ElementId elementId)
    {
        return _elementInfos.TryGetOrSetValue(
            elementId,
            () => service.GetElement(constructId, elementId),
            info => info == null
        );
    }

    public Task<ElementId> GetCoreUnit(ulong constructId)
    {
        return _coreUnits.TryGetOrSetValue(
            constructId,
            () => service.GetCoreUnit(constructId)
        );
    }

    public IConstructElementsService NoCache()
    {
        return service;
    }
}