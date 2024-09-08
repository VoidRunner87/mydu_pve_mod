using System;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

[AttributeUsage(AttributeTargets.Class)]
public class ScriptActionNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public string Description { get; set; }
    public bool VisibleOnUI { get; set; } = false;
}