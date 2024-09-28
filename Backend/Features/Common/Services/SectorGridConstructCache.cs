using System.Collections.Concurrent;
using System.Collections.Generic;
using Mod.DynamicEncounters.Common.Vector;
using Mod.DynamicEncounters.Features.Sector.Services;

namespace Mod.DynamicEncounters.Features.Common.Services;

public static class SectorGridConstructCache
{
    public static object Lock = new();
    public static ConcurrentDictionary<LongVector3, ConcurrentBag<ulong>> Data { get; set; } = new();

    public static HashSet<ulong> FindAroundGrid(LongVector3 grid)
    {
        const long gridSnap = (long)SectorPoolManager.SectorGridSnap;
        var offsets = GetOffsets(gridSnap);

        HashSet<ulong> result = [];

        foreach (var offset in offsets)
        {
            var searchGrid = grid + offset;
            if (!Data.TryGetValue(searchGrid, out var bag))
            {
                continue;
            }

            foreach (var constructId in bag)
            {
                result.Add(constructId);
            }
        }

        return result;
    }
    
    public static IEnumerable<LongVector3> GetOffsets(long gridSnap, int radius = 1)
    {
        var offsets = new List<LongVector3>();

        for (long x = -radius; x <= radius; x++)
        {
            for (long y = -radius; y <= radius; y++)
            {
                for (long z = -radius; z <= radius; z++)
                {
                    offsets.Add(new LongVector3(x * gridSnap, y * gridSnap, z * gridSnap));
                }
            }
        }

        return offsets;
    }
}