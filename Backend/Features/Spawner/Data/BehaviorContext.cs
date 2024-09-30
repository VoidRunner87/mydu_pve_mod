using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotLib.BotClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Events.Data;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class BehaviorContext(
    long factionId,
    Guid? territoryId,
    Vec3 sector,
    Client client,
    IServiceProvider serviceProvider,
    IPrefab prefab
)
{
    public ulong? TargetConstructId { get; set; }
    public Vec3 TargetMovePosition { get; set; }
    public IEnumerable<Vec3> TargetElementPositions { get; set; } = [];
    private double _deltaTime;

    public double DeltaTime
    {
        get => _deltaTime;
        set => _deltaTime = Math.Clamp(value, 1 / 60f, 1 / 30f);
    }

    public Dictionary<string, object> ExtraProperties = new();

    public Vec3 Velocity { get; set; }
    public Vec3? Position { get; set; }
    public Quat Rotation { get; set; }
    public ConcurrentDictionary<ulong, ulong> PlayerIds { get; set; } = new();
    public long FactionId { get; } = factionId;
    public Guid? TerritoryId { get; } = territoryId;
    public Vec3 Sector { get; } = sector;
    public IServiceProvider ServiceProvider { get; init; } = serviceProvider;
    public Client Client { get; set; } = client;

    public ConcurrentDictionary<string, bool> PublishedEvents = [];
    public IPrefab Prefab { get; set; } = prefab;
    public List<Waypoint> Waypoints { get; set; } = [];
    public Waypoint? TargetWaypoint { get; set; }

    public DateTime? TargetSelectedTime { get; set; }

    public bool IsAlive { get; set; } = true;

    public bool IsActiveWreck { get; set; }

    public Task NotifyEvent(string @event, BehaviorEventArgs eventArgs)
    {
        // TODO for custom events
        return Task.CompletedTask;
    }

    public async Task NotifyCoreStressHighAsync(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.ContainsKey(nameof(NotifyCoreStressHighAsync)))
        {
            return;
        }

        await Prefab.Events.OnCoreStressHigh.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.ServiceProvider,
                eventArgs.Context.FactionId,
                eventArgs.Context.PlayerIds.Select(x => x.Key).ToHashSet(),
                eventArgs.Context.Sector,
                eventArgs.Context.TerritoryId
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        PublishedEvents.TryAdd(nameof(NotifyCoreStressHighAsync), true);
    }

    public async Task NotifyConstructDestroyedAsync(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.ContainsKey(nameof(NotifyConstructDestroyedAsync)))
        {
            return;
        }

        var eventService = ServiceProvider.GetRequiredService<IEventService>();

        var taskList = new List<Task>();

        // send event for all players piloting constructs
        // TODO #limitation = not considering gunners and boarders
        var logger = eventArgs.Context.ServiceProvider.CreateLogger<BehaviorContext>();

        logger.LogInformation("NPC Defeated by players: {Players}", string.Join(", ", eventArgs.Context.PlayerIds));

        var tasks = eventArgs.Context.PlayerIds.Select(id =>
            eventService.PublishAsync(
                new PlayerDefeatedNpcEvent(
                    id.Key,
                    eventArgs.Context.Sector,
                    eventArgs.ConstructId,
                    eventArgs.Context.FactionId,
                    eventArgs.Context.PlayerIds.Count
                )
            )
        );

        taskList.AddRange(tasks);

        var scriptExecutionTask = Prefab.Events.OnDestruction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.ServiceProvider,
                eventArgs.Context.FactionId,
                eventArgs.Context.PlayerIds.Select(x => x.Key).ToHashSet(),
                eventArgs.Context.Sector,
                eventArgs.Context.TerritoryId
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        taskList.Add(scriptExecutionTask);

        await Task.WhenAll(taskList);

        PublishedEvents.TryAdd(nameof(NotifyConstructDestroyedAsync), true);
    }

    public async Task NotifyShieldHpHalfAsync(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.ContainsKey(nameof(NotifyShieldHpHalfAsync)))
        {
            return;
        }

        await Prefab.Events.OnShieldHalfAction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.ServiceProvider,
                eventArgs.Context.FactionId,
                eventArgs.Context.PlayerIds.Select(x => x.Key).ToHashSet(),
                eventArgs.Context.Sector,
                eventArgs.Context.TerritoryId
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        PublishedEvents.TryAdd(nameof(NotifyShieldHpHalfAsync), true);
    }

    public async Task NotifyShieldHpLowAsync(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.ContainsKey(nameof(NotifyShieldHpLowAsync)))
        {
            return;
        }

        await Prefab.Events.OnShieldLowAction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.ServiceProvider,
                eventArgs.Context.FactionId,
                eventArgs.Context.PlayerIds.Select(x => x.Key).ToHashSet(),
                eventArgs.Context.Sector,
                eventArgs.Context.TerritoryId
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        PublishedEvents.TryAdd(nameof(NotifyShieldHpLowAsync), true);
    }

    public async Task NotifyShieldHpDownAsync(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.ContainsKey(nameof(NotifyShieldHpDownAsync)))
        {
            return;
        }

        await Prefab.Events.OnShieldDownAction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.ServiceProvider,
                eventArgs.Context.FactionId,
                eventArgs.Context.PlayerIds.Select(x => x.Key).ToHashSet(),
                eventArgs.Context.Sector,
                eventArgs.Context.TerritoryId
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        PublishedEvents.TryAdd(nameof(NotifyShieldHpDownAsync), true);
    }

    public void Deactivate<T>() where T : IConstructBehavior
    {
        var name = typeof(T).FullName;
        var key = $"{name}_FINISHED";

        if (!ExtraProperties.TryAdd(key, false))
        {
            ExtraProperties[key] = false;
        }
    }

    public bool IsBehaviorActive<T>() where T : IConstructBehavior
    {
        return IsBehaviorActive(typeof(T));
    }

    public bool IsBehaviorActive(Type type)
    {
        var name = type.FullName;
        var key = $"{name}_FINISHED";

        if (ExtraProperties.TryGetValue(key, out var finished) && finished is bool finishedBool)
        {
            return !finishedBool;
        }

        return true;
    }

    public bool TryGetProperty<T>(string name, out T value, T defaultValue)
    {
        if (ExtraProperties.TryGetValue(name, out var objVal))
        {
            value = (T)objVal;
            return true;
        }

        value = defaultValue;
        return false;
    }

    public void SetProperty<T>(string name, T value)
    {
        if (!ExtraProperties.TryAdd(name, value))
        {
            ExtraProperties[name] = value;
        }
    }
}