using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Database;
using Backend.Scenegraph;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQutils.Def;
using NQutils.Sql;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class SafeZoneService(IServiceProvider provider) : ISafeZoneService
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions
    {
        ExpirationScanFrequency = TimeSpan.FromMinutes(10)
    });

    public async Task<IEnumerable<ISafeZoneService.SafeZoneSphere>> GetSafeZones()
    {
        if (_cache.TryGetValue(0, out var result))
        {
            return ((IEnumerable<ISafeZoneService.SafeZoneSphere>)result)!;
        }

        var data = (await GetSafeZonesRefresh()).ToList();

        _cache.Set(0, data);

        return data;
    }

    private async Task<IEnumerable<ISafeZoneService.SafeZoneSphere>> GetSafeZonesRefresh()
    {
        var bank = provider.GetGameplayBank();
        var sql = provider.GetRequiredService<ISql>();
        var sceneGraph = provider.GetRequiredService<IScenegraph>();

        var pvp = bank.GetBaseObject<PVPConfig>();
        var planetList = await sql.GetPlanetList();
        var planetMap = planetList
            .DistinctBy(x => x.Name)
            .ToDictionary(k => k.Name, v => v);

        var result = new List<ISafeZoneService.SafeZoneSphere>();

        foreach (var property in pvp.PlanetProperties)
        {
            if (planetMap.TryGetValue(property.PlanetName, out var planet) && planet.Id > 0)
            {
                result.Add(new ISafeZoneService.SafeZoneSphere
                {
                    Position = await sceneGraph.GetConstructCenterWorldPosition(planet.Id.Value),
                    Radius = property.AtmosphericRadius
                });
            }
        }

        foreach (var sz in pvp.SafeZones)
        {
            result.Add(new ISafeZoneService.SafeZoneSphere
            {
                Position = new Vec3
                {
                    x = sz.centerX,
                    y = sz.centerY,
                    z = sz.centerZ
                },
                Radius = sz.Radius
            });
        }

        return result;
    }
}