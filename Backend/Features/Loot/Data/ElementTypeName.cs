namespace Mod.DynamicEncounters.Features.Loot.Data;

public readonly struct ElementTypeName(string name)
{
    public string Name { get; } = name;
}