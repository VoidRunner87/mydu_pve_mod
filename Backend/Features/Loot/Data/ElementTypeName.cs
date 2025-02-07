namespace Mod.DynamicEncounters.Features.Loot.Data;

public readonly struct ElementTypeName(string name)
{
    public string Name { get; } = name;

    public static implicit operator string(ElementTypeName name) => name.Name;
    public static implicit operator ElementTypeName(string name) => new(name);
    public bool IsValid() => !string.IsNullOrEmpty(Name);
}