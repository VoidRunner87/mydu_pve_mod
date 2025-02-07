using System;
using System.Collections.Concurrent;

namespace Mod.DynamicEncounters;

public static class LoopStats
{
    public static ConcurrentDictionary<string, DateTime> LastHeartbeatMap { get; } = new();
}