using System;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters;

public static class ConstructBehaviorContextCache
{
    public static TemporaryMemoryCache<ulong, BehaviorContext> Data { get; set; } = new(nameof(ConstructBehaviorContextCache), TimeSpan.FromHours(3));

    private static readonly object Lock = new();
    public static bool IsBotDisconnected { get; set; }
    private static DateTime? LastTimeBotDisconnected { get; set; }

    public static void RaiseBotDisconnected()
    {
        lock (Lock)
        {
            var now = DateTime.UtcNow;
            
            if (LastTimeBotDisconnected == null || now - LastTimeBotDisconnected > TimeSpan.FromSeconds(5))
            {
                IsBotDisconnected = true;
                LastTimeBotDisconnected = DateTime.UtcNow;
            }
        }
    }
    
    public static void RaiseBotReconnected()
    {
        lock (Lock)
        {
            IsBotDisconnected = false;
        }
    }
}