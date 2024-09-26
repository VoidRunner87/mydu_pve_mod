using System.Collections.Concurrent;
using Mod.DynamicEncounters.Common.Vector;

namespace Mod.DynamicEncounters.Features.Common.Services;

public static class SectorGridConstructCache
{
    public static object Lock = new();
    public static ConcurrentDictionary<LongVector3, ConcurrentBag<ulong>> Data { get; set; } = new();
}