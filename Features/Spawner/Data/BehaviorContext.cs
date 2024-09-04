using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BotLib.BotClient;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class BehaviorContext(
    Vec3 sector,
    Client client,
    IServiceProvider serviceProvider,
    IConstructDefinition constructDefinition
)
{
    public ulong? TargetConstructId { get; set; }
    private double _deltaTime;

    public double DeltaTime
    {
        get => _deltaTime;
        set => _deltaTime = Math.Clamp(value, 1 / 60f, 1 / 30f);
    }

    public Dictionary<string, object> ExtraProperties = new();

    public Vec3 Velocity { get; set; }
    public Vec3 Position { get; set; }
    public HashSet<ulong> PlayerIds { get; set; } = new();
    public Vec3 Sector { get; } = sector;
    public IServiceProvider ServiceProvider { get; init; } = serviceProvider;
    public Client Client { get; set; } = client;

    public HashSet<string> PublishedEvents = [];
    public IConstructDefinition ConstructDefinition { get; set; } = constructDefinition;

    public DateTime? TargetSelectedTime { get; set; }

    public bool IsAlive { get; set; }

    public bool IsActiveWreck { get; set; }

    public virtual Task NotifyEvent(string @event, BehaviorEventArgs eventArgs)
    {
        // TODO for custom events
        return Task.CompletedTask;
    }
    
    public virtual async Task NotifyCoreStressHighAsync(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.Contains(nameof(NotifyCoreStressHighAsync)))
        {
            return;
        }

        await ConstructDefinition.Events.OnCoreStressHigh.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.ServiceProvider,
                eventArgs.Context.PlayerIds,
                eventArgs.Context.Sector
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        PublishedEvents.Add(nameof(NotifyCoreStressHighAsync));
    }

    public virtual async Task NotifyConstructDestroyedAsync(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.Contains(nameof(NotifyConstructDestroyedAsync)))
        {
            return;
        }

        await ConstructDefinition.Events.OnDestruction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.ServiceProvider,
                eventArgs.Context.PlayerIds,
                eventArgs.Context.Sector
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        PublishedEvents.Add(nameof(NotifyConstructDestroyedAsync));
    }

    public virtual async Task NotifyShieldHpHalfAsync(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.Contains(nameof(NotifyShieldHpHalfAsync)))
        {
            return;
        }

        await ConstructDefinition.Events.OnShieldHalfAction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.ServiceProvider,
                eventArgs.Context.PlayerIds,
                eventArgs.Context.Sector
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        PublishedEvents.Add(nameof(NotifyShieldHpHalfAsync));
    }

    public virtual async Task NotifyShieldHpLowAsync(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.Contains(nameof(NotifyShieldHpLowAsync)))
        {
            return;
        }

        await ConstructDefinition.Events.OnShieldLowAction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.ServiceProvider,
                eventArgs.Context.PlayerIds,
                eventArgs.Context.Sector
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        PublishedEvents.Add(nameof(NotifyShieldHpLowAsync));
    }

    public virtual async Task NotifyShieldHpDownAsync(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.Contains(nameof(NotifyShieldHpDownAsync)))
        {
            return;
        }

        await ConstructDefinition.Events.OnShieldDownAction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.ServiceProvider,
                eventArgs.Context.PlayerIds,
                eventArgs.Context.Sector
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        PublishedEvents.Add(nameof(NotifyShieldHpDownAsync));
    }
}