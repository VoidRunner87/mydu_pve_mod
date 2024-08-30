using System.Collections.Generic;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ScriptActionItem
{
    public string? Name { get; set; }
    public string Type { get; set; }
    public string Prefab { get; set; }
    public int MinQuantity { get; set; } = 1;
    public int MaxQuantity { get; set; } = 1;
    public string Message { get; set; }
    public string Script { get; set; }
    public ulong ConstructId { get; set; }
    public Vec3? Position { get; set; } = new();

    public ScriptActionAreaItem Area { get; set; } = new() { Type = "sphere", Radius = 1 };
    public List<ScriptActionItem> Actions { get; set; } = new();

    public ScriptActionEvents Events { get; set; } = new();
}