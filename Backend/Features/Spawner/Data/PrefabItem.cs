using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class PrefabItem
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string RandomNameGroup { get; set; } = string.Empty;
    public bool UseRandomNameGroup { get; set; } = false;
    public string Folder { get; set; }
    public string Path { get; set; }
    public ulong OwnerId { get; set; }
    public bool IsUntargetable { get; set; }
    public bool IsNpc { get; set; }
    public ServerPropertiesItem ServerProperties { get; set; } = new();
    public BehaviorModifiers Mods { get; set; } = new();
    public List<string> InitialBehaviors { get; set; } = [];
    public int AmmoTier { get; set; } = 3;
    public string AmmoVariant { get; set; } = "Agile";
    public double AccelerationG { get; set; } = 15;
    public float RotationSpeed { get; set; } = 0.5f;
    public double MinSpeedKph { get; set; } = 2000;
    public double MaxSpeedKph { get; set; } = 20000;
    public double TargetDecisionTimeSeconds { get; set; } = 30;
    public double TargetDistance { get; set; } = 20000;
    public long FactionId { get; set; }
    public double RealismFactor { get; set; }
    public int MaxWeaponCount { get; set; } = 2;
    public bool DamagesVoxel { get; set; } = true;
    public bool UsesCustomShootAction { get; set; } = true;
    public double SectorExpirationSeconds { get; set; } = 60 * 30;

    public PrefabEvents Events { get; set; } = new();

    public IEnumerable<object> Skills { get; set; } = [];

    public class ServerPropertiesItem
    {
        public bool IsDynamicWreck { get; set; }
        public HeaderProp Header { get; set; } = new();

        public class HeaderProp
        {
            public string PrettyName { get; set; }
        }
    }
}