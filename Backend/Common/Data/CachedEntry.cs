using System;

namespace Mod.DynamicEncounters.Common.Data;

public class CachedEntry<T>(T data, DateTime expiresAt)
{
    public DateTime ExpiresAt { get; } = expiresAt;
    public T Data { get; set; } = data;

    public bool IsExpired(DateTime now)
    {
        return now >= ExpiresAt;
    }
}