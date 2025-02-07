namespace Mod.DynamicEncounters.Features.Faction.Data;

public readonly struct FactionId(long id)
{
    public long Id { get; } = id;
    
    public static implicit operator FactionId(long id) => new(id);

    public static implicit operator long(FactionId factionId) => factionId.Id;

    public override string ToString() => $"{Id}";
}