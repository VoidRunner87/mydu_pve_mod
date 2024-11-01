using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class BehaviorContext(
    ulong constructId,
    long factionId,
    Guid? territoryId,
    Vec3 sector,
    IServiceProvider serviceProvider,
    IPrefab prefab
) : BaseContext
{
    [Newtonsoft.Json.JsonIgnore]
    [JsonIgnore]
    public IEnumerable<Vec3> TargetElementPositions { get; set; } = [];

    private double _deltaTime;

    public double DeltaTime
    {
        get => _deltaTime;
        set => _deltaTime = Math.Clamp(value, 1 / 60f, 1 / 20f);
    }

    public const string AutoTargetMovePositionEnabledProperty = "AutoTargetMovePositionEnabled";
    public const string AutoSelectAttackTargetConstructProperty = "AutoSelectAttackTargetConstruct";
    public const string EnginePowerProperty = "EnginePower";
    public const string IdleSinceProperty = "IdleSince";
    public const string V0Property = "V0";
    public const string BrakingProperty = "Braking";
    public const string MoveModeProperty = "MoveMode";

    public DateTime StartedAt { get; } = DateTime.UtcNow;
    public Vec3 Velocity { get; set; }
    public Vec3? Position { get; set; }
    public Quat Rotation { get; set; }
    public HashSet<ulong> PlayerIds { get; set; } = new();
    public ulong ConstructId { get; } = constructId;
    public long FactionId { get; } = factionId;
    public Guid? TerritoryId { get; } = territoryId;
    public Vec3 Sector { get; } = sector;
    public IServiceProvider ServiceProvider { get; init; } = serviceProvider;

    public ConcurrentDictionary<string, bool> PublishedEvents = [];

    public ConcurrentDictionary<string, TimerPropertyValue> PropertyOverrides { get; } = [];

    [Newtonsoft.Json.JsonIgnore]
    [JsonIgnore]
    public IPrefab Prefab { get; set; } = prefab;

    public DateTime? TargetSelectedTime { get; set; }

    public bool IsAlive { get; set; } = true;

    public bool IsActiveWreck { get; set; }

    public double RealismFactor { get; set; } = prefab.DefinitionItem.RealismFactor;

    public Task NotifyEvent(string @event, BehaviorEventArgs eventArgs)
    {
        // TODO for custom events
        return Task.CompletedTask;
    }

    public void Deactivate<T>() where T : IConstructBehavior
    {
        var name = typeof(T).FullName;
        var key = $"{name}_FINISHED";

        if (!Properties.TryAdd(key, false))
        {
            Properties[key] = false;
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

        if (Properties.TryGetValue(key, out var finished) && finished is bool finishedBool)
        {
            return !finishedBool;
        }

        return true;
    }

    public void SetTargetMovePosition(Vec3 position)
    {
        Properties.Set(nameof(DynamicProperties.TargetMovePosition), position);
    }

    public Vec3 GetTargetMovePosition()
    {
        return this.GetOverrideOrDefault(
            nameof(DynamicProperties.TargetMovePosition),
            new Vec3()
        );
        
        // return (Vec3)Properties.GetOrDefault(nameof(DynamicProperties.TargetMovePosition), new Vec3());
    }

    public ulong? GetTargetConstructId()
    {
        return this.GetOverrideOrDefault(
            nameof(DynamicProperties.TargetConstructId),
            (ulong?)null
        );
    }

    public void SetTargetConstructId(ulong? constructId)
    {
        // can't target itself
        if (constructId == ConstructId)
        {
            return;
        }

        Properties.Set(nameof(DynamicProperties.TargetConstructId), constructId);
        Properties.Set(nameof(DynamicProperties.TargetSelectedTime), DateTime.UtcNow);
    }

    public void SetWaypointList(IEnumerable<Waypoint> waypoints)
    {
        Properties.Set(nameof(DynamicProperties.WaypointList), waypoints.ToList());
    }

    public Waypoint? GetNextNotVisited()
    {
        return GetWaypointList().FirstOrDefault(x => !x.Visited);
    }

    public object GetUnparsedWaypointList()
    {
        TryGetProperty(
            nameof(DynamicProperties.WaypointList),
            out object waypointList,
            new List<Waypoint>()
        );

        return waypointList;
    }

    public double CalculateBrakingDistance()
    {
        var velSize = Velocity.Size();
        var brakingAcceleration = Prefab.DefinitionItem.AccelerationG * 9.81f;
        
        return velSize * velSize / (2 * brakingAcceleration);
    }

    public IEnumerable<Waypoint> GetWaypointList()
    {
        return this.GetOverrideOrDefault(
            nameof(DynamicProperties.WaypointList),
            (List<Waypoint>?) []
        );
    }

    public bool IsWaypointListInitialized()
    {
        TryGetProperty(
            nameof(DynamicProperties.WaypointListInitialized),
            out var initDone,
            false
        );

        return initDone;
    }

    public void TagWaypointListInitialized()
    {
        SetProperty(
            nameof(DynamicProperties.WaypointListInitialized),
            true
        );
    }

    public void ClearExpiredTimerProperties()
    {
        var expiredList = PropertyOverrides
            .Where(kvp => kvp.Value.IsExpired(DateTime.UtcNow))
            .ToList();

        foreach (var kvp in expiredList)
        {
            PropertyOverrides.TryRemove(kvp.Key, out _);
        }
    }

    private static class DynamicProperties
    {
        public const byte TargetMovePosition = 1;
        public const byte TargetConstructId = 2;
        public const byte TargetSelectedTime = 3;
        public const byte WaypointList = 4;
        public const byte WaypointListInitialized = 5;
    }

    public class TimerPropertyValue(DateTime expiresAt, object? value)
    {
        public DateTime ExpiresAt { get; } = expiresAt;
        public object? Value { get; } = value;

        public bool IsExpired(DateTime now)
        {
            return now > ExpiresAt;
        }
    }
}