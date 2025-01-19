using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Vector.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class BehaviorContext(
    ulong constructId,
    long factionId,
    Guid? territoryId,
    Vec3 sector,
    IServiceProvider provider,
    IPrefab prefab
) : BaseContext
{
    private ISkillFactory SkillFactory => Provider.GetRequiredService<ISkillFactory>();

    private double _deltaTime;

    public double DeltaTime
    {
        get => _deltaTime;
        set => _deltaTime = Math.Clamp(value, 1 / 60f, 1);
    }

    public const string AutoTargetMovePositionEnabledProperty = "AutoTargetMovePositionEnabled";
    public const string AutoSelectAttackTargetConstructProperty = "AutoSelectAttackTargetConstruct";
    public const string EnginePowerProperty = "EnginePower";
    public const string IdleSinceProperty = "IdleSince";
    public const string V0Property = "V0";
    public const string BrakingProperty = "Braking";
    public const string MoveModeProperty = "MoveMode";

    public TimeSpan? ActiveSectorExpirationSeconds { get; } =
        TimeSpan.FromSeconds(prefab.DefinitionItem.SectorExpirationSeconds);

    public bool CustomActionShootEnabled { get; } = prefab.DefinitionItem.UsesCustomShootAction;
    public bool DamagesVoxel { get; } = prefab.DefinitionItem.DamagesVoxel;
    public DateTime StartedAt { get; } = DateTime.UtcNow;
    public Vec3 Velocity { get; set; }
    public Vec3? Position { get; private set; }
    public Vec3? StartPosition { get; private set; }
    public Quat Rotation { get; set; }
    public float TargetRotationPositionMultiplier { get; set; } = 1;
    public HashSet<ulong> PlayerIds { get; set; } = [];
    public ulong ConstructId { get; } = constructId;
    public long FactionId { get; } = factionId;
    public Guid? TerritoryId { get; } = territoryId;
    public Vec3 Sector { get; } = sector;
    public Vec3 AccelCalcTargetPosition { get; set; }
    public Vec3 AccelCalcTargetVelocity { get; set; }
    public Vec3 TargetPosition { get; set; }
    public Vec3 TargetMovePosition { get; private set; }
    public Vec3? OverrideTargetMovePosition { get; private set; }

    public ConcurrentBag<ScanContact> Contacts { get; private set; } = [];
    public ConcurrentBag<DamageDealtData> DamageHistory { get; private set; } = [];

    private bool _skillsInitialized;
    private IList<ISkill> _skills = [];

    public IList<ISkill> Skills
    {
        get
        {
            if (!_skillsInitialized)
            {
                _skills = SkillFactory.CreateAll(prefab.DefinitionItem.Skills).ToList();
                _skillsInitialized = true;
            }

            return _skills;
        }
        set => _skills = value;
    }

    public bool BoosterActive { get; set; } = false;
    public double AccelerationG { get; set; } = prefab.DefinitionItem.AccelerationG;

    public void RegisterDamage(DamageDealtData data)
    {
        DamageHistory = new ConcurrentBag<DamageDealtData>(GetRecentDamageHistory()) { data };
    }

    public IEnumerable<DamageDealtData> GetRecentDamageHistory()
    {
        return DamageHistory.Where(x => x.DateTime > DateTime.UtcNow - TimeSpan.FromMinutes(10));
    }

    public double GetTotalDamageFromHistory() => DamageHistory.Sum(x => x.Damage);

    public Dictionary<ulong, double> GetTotalDamageByPlayer() => DamageHistory.GroupBy(x => x.PlayerId)
        .Select(x => new
        {
            PlayerId = x.Key,
            Sum = x.Sum(d => d.Damage)
        })
        .ToDictionary(k => k.PlayerId, v => v.Sum);

    public ulong? GetHighestThreatConstruct()
    {
        var damageHistory = DamageHistory
            .Where(x => x.DateTime > DateTime.UtcNow - TimeSpan.FromMinutes(1))
            .GroupBy(x => x.ConstructId)
            .Select(x => new
            {
                ConstructId = x.Key,
                TotalDamage = x.Sum(d => d.Damage)
            })
            .OrderByDescending(x => x.TotalDamage);

        var highestThreat = damageHistory.FirstOrDefault();
        if (highestThreat == null)
        {
            return GetClosestTarget();
        }

        return highestThreat.ConstructId;
    }

    public ulong? GetClosestTarget()
    {
        if (Contacts.IsEmpty)
        {
            return null;
        }

        return Contacts.MinBy(x => x.Distance).ConstructId;
    }

    public double GetAccelerationG()
    {
        var boosterG = 0d;
        if (Modifiers.Velocity.BoosterEnabled && BoosterActive)
        {
            boosterG = Modifiers.Velocity.BoosterAccelerationG;
        }

        return AccelerationG + boosterG;
    }

    public double GetAccelerationMps() => GetAccelerationG() * 3.6d;

    public Vec3 TargetAcceleration { get; private set; }
    public DateTime LastTargetAccelerationUpdate { get; private set; } = DateTime.UtcNow;
    private ulong? TargetConstructId { get; set; }
    public double TargetDistance { get; set; }
    public Vec3 TargetLinearVelocity { get; private set; }
    public double VelocityWithTargetDotProduct { get; private set; }
    public bool IsApproaching { get; set; }
    public DateTime? LastApproachingUpdate { get; set; }
    public double MinVelocity { get; set; } = prefab.DefinitionItem.MinSpeedKph / 3.6d;
    public double MaxVelocity { get; set; } = prefab.DefinitionItem.MaxSpeedKph / 3.6d;
    public double TargetMoveDistance { get; private set; }
    public double ShotWaitTime { get; set; }
    public ConstructDamageData DamageData { get; set; } = new([]);
    public Dictionary<string, List<WeaponEffectivenessData>> WeaponEffectivenessData { get; set; } = new();
    public ConcurrentDictionary<ulong, ConstructDamageData> TargetDamageData { get; set; } = new();
    public IServiceProvider Provider { get; init; } = provider;
    public readonly ConcurrentDictionary<string, bool> PublishedEvents = [];
    public ConcurrentDictionary<string, TimerPropertyValue> PropertyOverrides { get; } = [];
    public IEffectHandler Effects { get; set; } = new EffectHandler(provider);
    public BehaviorModifiers Modifiers { get; set; } = prefab.DefinitionItem.Mods;

    [Newtonsoft.Json.JsonIgnore]
    [JsonIgnore]
    public IPrefab Prefab => prefab;

    public DateTime? TargetSelectedTime { get; set; }

    public bool IsAlive { get; set; } = true;

    public bool IsActiveWreck { get; set; }

    public double RealismFactor { get; set; } = prefab.DefinitionItem.RealismFactor;
    public bool HasShield { get; set; }
    public double ShieldPercent { get; set; } = 0;
    public bool IsShieldActive { get; set; }
    public bool IsShieldVenting { get; set; }
    public double FunctionalWeaponFactor { get; set; }
    public bool OverridePilotTakeOver { get; set; }

    public void UpdateShieldState(ConstructInfo constructInfo)
    {
        HasShield = constructInfo.mutableData.shieldState.hasShield;
        ShieldPercent = constructInfo.mutableData.shieldState.shieldHpRatio;
        IsShieldActive = constructInfo.mutableData.shieldState.isActive;
        IsShieldVenting = constructInfo.mutableData.shieldState.isVenting;
    }

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

    public bool HasAnyDamagingWeapons()
    {
        return GetAvailableWeapons().Any();
    }

    public bool HasAnyWeapons()
    {
        return GetAvailableWeapons().Any();
    }

    public IEnumerable<WeaponEffectivenessData> GetAvailableWeapons()
    {
        return WeaponEffectivenessData
            .SelectMany(x => x.Value).Where(x => !x.IsDestroyed());
    }

    public (int functionalCount, int totalCount) GetWeaponEffectivenessFactors(string itemTypeName)
    {
        if (!WeaponEffectivenessData.TryGetValue(itemTypeName, out var list))
        {
            return (0, 1);
        }

        if (list.Count == 0)
        {
            return (0, 1);
        }

        var functionalCount = list.Count(x => !x.IsDestroyed());
        var totalCount = list.Count;

        return (functionalCount, totalCount);
    }

    public WeaponItem? GetBestFunctionalWeaponByTargetDistance(double targetDistance)
    {
        var availableWeapons = GetAvailableWeapons()
            .Select(w => w.Name);
        var weapons = DamageData.Weapons
            .Where(w => availableWeapons.Contains(w.ItemTypeName));

        var damageTrait = new ConstructDamageData(weapons);
        return damageTrait.GetBestWeaponByTargetDistance(targetDistance);
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

    public void SetPosition(Vec3 position)
    {
        if (!StartPosition.HasValue)
        {
            StartPosition = position;
        }

        Position = position;
    }

    public void SetTargetMovePosition(Vec3 position)
    {
        TargetMovePosition = position;
    }

    public void SetOverrideTargetMovePosition(Vec3? position)
    {
        OverrideTargetMovePosition = position;
    }

    public double GetMovePositionDistance()
    {
        if (!Position.HasValue) return 0;

        return OverrideTargetMovePosition?.Dist(Position.Value) ?? TargetDistance;
    }

    public void SetTargetLinearVelocity(Vec3 linear)
    {
        TargetLinearVelocity = linear;

        var targetDirection = TargetLinearVelocity.NormalizeSafe();
        var npcDirection = Velocity.NormalizeSafe();

        VelocityWithTargetDotProduct = npcDirection.Dot(targetDirection);
    }

    public void SetIsApproachingTarget(double previousDistance, double currentDistance)
    {
        IsApproaching = previousDistance > currentDistance;
    }

    public bool IsApproachingTarget()
    {
        return IsApproaching;
    }

    public void SetTargetPosition(Vec3 targetPosition)
    {
        var deltaTime = (DateTime.UtcNow - LastTargetAccelerationUpdate).TotalSeconds;

        TargetPosition = targetPosition;
        if (deltaTime > 1)
        {
            var acceleration = VelocityHelper.CalculateAcceleration(
                AccelCalcTargetPosition,
                targetPosition,
                AccelCalcTargetVelocity,
                deltaTime
            );

            AccelCalcTargetPosition = targetPosition;
            AccelCalcTargetVelocity = TargetLinearVelocity;
            TargetAcceleration = acceleration;
            LastTargetAccelerationUpdate = DateTime.UtcNow;
        }
    }

    public bool IsInsideOptimalRange() => TargetDistance <= GetBestWeaponOptimalRange();
    public bool IsOutsideOptimalRange() => TargetDistance > GetBestWeaponOptimalRange();
    public bool IsOutsideDoubleOptimalRange() => TargetDistance > GetBestWeaponOptimalRange() * 2;

    public double GetBestWeaponOptimalRange()
    {
        if (!Position.HasValue) return 0;

        var weaponItem = DamageData.GetBestWeaponByTargetDistance(
            TargetPosition.Dist(Position.Value)
        );

        if (weaponItem == null) return 0;

        return DamageData.GetHalfFalloffFiringDistance(weaponItem);
    }

    public void SetTargetDistance(double distance)
    {
        SetIsApproachingTarget(TargetDistance, distance);

        TargetDistance = distance;
    }

    public void SetTargetMoveDistance(double distance)
    {
        TargetMoveDistance = distance;
    }

    public Vec3 GetTargetMovePosition()
    {
        return OverrideTargetMovePosition ?? TargetMovePosition;
    }

    public Vec3 GetTargetPosition() => TargetPosition;

    public bool HasTargetConstruct() => this.TargetConstructId.HasValue;
    public ulong? GetTargetConstructId() => this.TargetConstructId;

    private double GetOutsideOfOptimalRange2XTargetVelocity()
    {
        if (TargetLinearVelocity.Size() < MinVelocity)
        {
            return MaxVelocity / Prefab.DefinitionItem.Mods.Velocity.OutsideOptimalRange2XAlpha;
        }

        return Math.Clamp(
            TargetLinearVelocity.Size(),
            MinVelocity,
            MaxVelocity
        );
    }

    private double GetOutsideOfOptimalRangeTargetVelocity()
    {
        if (TargetLinearVelocity.Size() < MinVelocity)
        {
            return MaxVelocity / Prefab.DefinitionItem.Mods.Velocity.OutsideOptimalRangeAlpha;
        }

        return Math.Clamp(
            TargetLinearVelocity.Size(),
            MinVelocity,
            MaxVelocity
        );
    }

    private double GetInsideOfOptimalRangeTargetVelocity()
    {
        if (TargetLinearVelocity.Size() < MinVelocity)
        {
            return MinVelocity;
        }

        return Math.Clamp(
            TargetLinearVelocity.Size(),
            MinVelocity,
            MaxVelocity
        );
    }

    public double CalculateOverrideMoveVelocityGoal()
    {
        var brakingDistance = CalculateBrakingDistance();
        var movePositionDistance = GetMovePositionDistance();

        if (movePositionDistance <= brakingDistance * Modifiers.Velocity.BrakeDistanceFactor)
        {
            return 0D;
        }

        return MaxVelocity;
    }

    public double CalculateVelocityGoal(double distance)
    {
        if (!Modifiers.Velocity.Enabled) return MaxVelocity;
        if (OverrideTargetMovePosition.HasValue) return CalculateOverrideMoveVelocityGoal();

        var oppositeVector = VelocityWithTargetDotProduct < 0;

        if (distance > Modifiers.Velocity.GetFarDistanceM())
        {
            return MaxVelocity;
        }

        var brakingDistance = CalculateBrakingDistance();

        if (IsOutsideDoubleOptimalRange() || distance > brakingDistance * Modifiers.Velocity.BrakeDistanceFactor)
        {
            if (oppositeVector)
            {
                return GetOutsideOfOptimalRange2XTargetVelocity() * Modifiers.Velocity.OutsideOptimalRange2X.Negative;
            }

            return GetOutsideOfOptimalRange2XTargetVelocity() * Modifiers.Velocity.OutsideOptimalRange2X.Positive;
        }

        if (IsOutsideOptimalRange())
        {
            if (oppositeVector)
            {
                return GetOutsideOfOptimalRangeTargetVelocity() * Modifiers.Velocity.OutsideOptimalRange.Negative;
            }

            return GetOutsideOfOptimalRangeTargetVelocity() * Modifiers.Velocity.OutsideOptimalRange.Positive;
        }

        if (distance < Modifiers.Velocity.TooCloseDistanceM)
        {
            return MaxVelocity;
        }

        if (oppositeVector)
        {
            return GetInsideOfOptimalRangeTargetVelocity() * Modifiers.Velocity.InsideOptimalRange.Negative;
        }

        return GetInsideOfOptimalRangeTargetVelocity() * Modifiers.Velocity.InsideOptimalRange.Positive;
    }

    public double CalculateMovementPredictionSeconds()
    {
        if (IsOutsideDoubleOptimalRange())
        {
            return 10;
        }

        if (IsOutsideOptimalRange())
        {
            return 30;
        }

        return 60;
    }

    public void SetTargetConstructId(ulong? constructId)
    {
        // can't target itself
        if (constructId == ConstructId)
        {
            return;
        }

        TargetConstructId = constructId;
        TargetSelectedTime = DateTime.UtcNow;
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

    public void UpdateRadarContacts(IList<ScanContact> contacts)
    {
        Contacts = new ConcurrentBag<ScanContact>(contacts);
    }

    public bool HasAnyRadarContact() => !Contacts.IsEmpty;

    public void RefreshIdleSince()
    {
        SetProperty(IdleSinceProperty, DateTime.UtcNow);
    }

    public void SetTargetDamageData(ulong constructId, ConstructDamageData data)
    {
        TargetDamageData.TryAdd(constructId, data);
    }

    public double CalculateBrakingDistance()
    {
        return VelocityHelper.CalculateBrakingDistance(
            Velocity.Size(),
            GetAccelerationMps()
        );
    }

    public double CalculateBrakingTime()
    {
        return VelocityHelper.CalculateBrakingTime(
            Velocity.Size(),
            GetAccelerationMps()
        );
    }

    public double CalculateAccelerationToTargetSpeedTime(double fromVelocity)
    {
        return VelocityHelper.CalculateTimeToReachVelocity(
            fromVelocity,
            TargetLinearVelocity.Size(),
            GetAccelerationMps()
        );
    }

    public double CalculateTimeToMergeToDistance(double distance)
    {
        if (!Position.HasValue) return double.PositiveInfinity;

        return VelocityHelper.CalculateTimeToReachDistance(
            Position.Value,
            Velocity,
            TargetPosition,
            TargetLinearVelocity,
            distance
        );
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

    public ScriptContext GetScriptContext()
    {
        return new ScriptContext(
            Provider,
            FactionId,
            PlayerIds,
            Sector,
            TerritoryId).WithConstructId(ConstructId);
    }
}