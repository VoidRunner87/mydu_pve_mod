using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class ConstructDefinitionItem
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Folder { get; set; }
    public string Path { get; set; }
    public ulong OwnerId { get; set; }
    public bool IsUntargetable { get; set; }
    public bool IsNpc { get; set; }
    public ServerPropertiesItem ServerProperties { get; set; }
    public BehaviorModifiers Mods { get; set; } = new();
    public List<string> InitialBehaviors { get; set; } = ["wreck"];

    public ConstructDefinitionEvents Events { get; set; } = new();

    public class ServerPropertiesItem
    {
        public bool IsDynamicWreck { get; set; }
        public HeaderProp Header { get; set; }

        public class HeaderProp
        {
            public string PrettyName { get; set; }
        }
    }
}