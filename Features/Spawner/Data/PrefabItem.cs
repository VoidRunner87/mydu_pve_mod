using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class PrefabItem
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Folder { get; set; }
    public string Path { get; set; }
    public ulong OwnerId { get; set; }
    public bool IsUntargetable { get; set; }
    public bool IsNpc { get; set; }
    public ServerPropertiesItem ServerProperties { get; set; } = new();
    public BehaviorModifiers Mods { get; set; } = new();
    public List<string> InitialBehaviors { get; set; } = [];
    public List<string> AmmoItems { get; set; } = [];
    public List<string> WeaponItems { get; set; } = [];
    public float AccelerationG { get; set; } = 15;
    public float MaxSpeedKph { get; set; } = 20000;
    public float TargetDistance { get; set; } = 20000;

    public ConstructDefinitionEvents Events { get; set; } = new();

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