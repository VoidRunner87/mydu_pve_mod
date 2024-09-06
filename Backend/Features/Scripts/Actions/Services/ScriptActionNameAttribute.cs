using System;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

[AttributeUsage(AttributeTargets.Class)]
public class ScriptActionNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}