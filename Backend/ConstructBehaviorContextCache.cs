using System;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters;

public static class ConstructBehaviorContextCache
{
    public static TemporaryMemoryCache<ulong, BehaviorContext> Data { get; set; } = new(nameof(ConstructBehaviorContextCache), TimeSpan.FromHours(3));
}