using System.Collections.Concurrent;

namespace Mod.DynamicEncounters.Features.Sector.Services;

public static class ConstructsPendingDelete
{
    public static ConcurrentQueue<ulong> Data { get; } = [];
}