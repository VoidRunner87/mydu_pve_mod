using System;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;

public interface IEffectHandler
{
    void RegisterDefault<T>(T effect);
    T? GetOrNull<T>() where T : IEffect;
    bool IsEffectActive<T>() where T : IEffect;
    void Activate<T>(T effect, TimeSpan duration);
    void Activate<T>(TimeSpan duration) where T : IEffect, new();
    void Deactivate<T>();
    void CleanupExpired();
}