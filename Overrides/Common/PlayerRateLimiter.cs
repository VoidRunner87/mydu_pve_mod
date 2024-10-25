using System;
using Microsoft.Extensions.Caching.Memory;
using NQ;

namespace Mod.DynamicEncounters.Overrides.Common;

public class PlayerRateLimiter(int requestsPerSecond)
{
    private readonly MemoryCache _requestTracker = new(
        new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromSeconds(1/10d)
        }
    );

    private readonly int _requestsPerSecond = Math.Clamp(requestsPerSecond, 1, 10);

    public void TrackRequest(PlayerId playerId)
    {
        var count = 0;

        if (_requestTracker.TryGetValue(playerId, out int currentCount))
        {
            count = currentCount;
        }

        count++;

        _requestTracker.Set(playerId, count, TimeSpan.FromSeconds(1d / _requestsPerSecond));
    }

    public bool ExceededRateLimit(PlayerId playerId)
    {
        return _requestTracker.TryGetValue(playerId, out int currentCount) && currentCount > _requestsPerSecond;
    }
}