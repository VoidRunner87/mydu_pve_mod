using System;

namespace Mod.DynamicEncounters.Features.Party.Data;

public readonly struct PlayerPartyGroupId(Guid id)
{
    public Guid Id { get; } = id;

    public bool Equals(PlayerPartyGroupId other)
    {
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is PlayerPartyGroupId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}