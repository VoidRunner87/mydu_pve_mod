using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class CachedAreaScanService(IAreaScanService areaScanService) : IAreaScanService
{
    private readonly TemporaryMemoryCache<ulong, IEnumerable<ScanContact>> _npcRadar =
        new(nameof(_npcRadar), TimeSpan.FromSeconds(2));

    public Task<IEnumerable<ScanContact>> ScanForPlayerContacts(ulong constructId, Vec3 position,
        double radius,
        int limit)
    {
        return _npcRadar.TryGetOrSetValue(
            constructId,
            () => areaScanService.ScanForPlayerContacts(constructId, position, radius)
        );
    }

    public Task<IEnumerable<ScanContact>> ScanForNpcConstructs(Vec3 position, double radius, int limit = 10)
    {
        return areaScanService.ScanForNpcConstructs(position, radius, limit);
    }

    public Task<IEnumerable<ScanContact>> ScanForAbandonedConstructs(Vec3 position, double radius, int limit = 10)
    {
        return areaScanService.ScanForAbandonedConstructs(position, radius, limit);
    }

    public Task<IEnumerable<ScanContact>> ScanForAsteroids(Vec3 position, double radius)
    {
        return areaScanService.ScanForAsteroids(position, radius);
    }

    public Task<IEnumerable<ScanContact>> ScanForPlanetaryBodies(Vec3 position, double radius)
    {
        return areaScanService.ScanForPlanetaryBodies(position, radius);
    }
}