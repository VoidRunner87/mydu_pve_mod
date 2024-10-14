using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class CachedAreaScanService(INpcRadarService npcRadarService) : INpcRadarService
{
    private readonly TemporaryMemoryCache<ulong, IEnumerable<NpcRadarContact>> _npcRadar =
        new(nameof(_npcRadar), TimeSpan.FromSeconds(3));

    public Task<IEnumerable<NpcRadarContact>> ScanForPlayerContacts(ulong constructId, Vec3 position,
        double radius,
        int limit)
    {
        return _npcRadar.TryGetOrSetValue(
            constructId,
            () => npcRadarService.ScanForPlayerContacts(constructId, position, radius)
        );
    }
}