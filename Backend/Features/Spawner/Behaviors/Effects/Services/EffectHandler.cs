using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Extensions;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;

public class EffectHandler : IEffectHandler
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private ConcurrentDictionary<Type, object> DefaultEffects { get; set; } = new();
    private ConcurrentDictionary<Type, EffectEntry> Effects { get; set; } = new();

    public EffectHandler(IServiceProvider provider)
    {
        _dateTimeProvider = provider.GetRequiredService<IDateTimeProvider>();
        
        RegisterDefault<ICalculateTargetMovePositionEffect>(new CalculateTargetMovePositionWithOffsetEffect(provider));
        RegisterDefault<IMovementEffect>(new BurnToTargetMovementEffect());
        RegisterDefault<ISelectRadarTargetEffect>(new HighestThreatRadarTargetEffect());
    }

    public void RegisterDefault<T>(T effect)
    {
        DefaultEffects.TryAdd(typeof(T), effect);
    }

    public T? GetOrNull<T>() where T : IEffect
    {
        if (Effects.TryGetValue(typeof(T), out var entry))
        {
            if (!entry.IsExpired(_dateTimeProvider.UtcNow()))
            {
                return entry.EffectAs<T>();
            }
        }

        if (DefaultEffects.TryGetValue(typeof(T), out var effect))
        {
            return (T)effect;
        }

        return default;
    }

    public bool IsEffectActive<T>() where T : IEffect
    {
        return GetOrNull<T>() != null;
    }

    public void Activate<T>(T effect, TimeSpan duration)
    {
        if (effect == null)
        {
            return;
        }

        var entry = new EffectEntry(effect, _dateTimeProvider.UtcNow() + duration);
        Effects.Set(typeof(T), entry);
    }

    public void Activate<T>(TimeSpan duration) where T : IEffect, new()
    {
        var effect = new T();

        Activate(effect, duration);
    }

    public void Deactivate<T>()
    {
        Effects.TryRemove(typeof(T), out _);
    }

    public void CleanupExpired()
    {
        foreach (var kvp in Effects.ToList())
        {
            if (kvp.Value.IsExpired(_dateTimeProvider.UtcNow()))
            {
                Effects.TryRemove(kvp.Key, out _);
            }
        }
    }

    private class EffectEntry(object effect, DateTime expiresAt)
    {
        private object Effect { get; set; } = effect;
        private DateTime ExpiresAt { get; set; } = expiresAt;

        public T EffectAs<T>() => (T)Effect;

        public bool IsExpired(DateTime dateTime) => dateTime > ExpiresAt;
    }
}