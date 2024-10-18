using System;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public struct QuestId(Guid id)
{
    public Guid Id { get; } = id;
    
    public static implicit operator QuestId(Guid id) => new(id);

    public static implicit operator Guid(QuestId factionId) => factionId.Id;
}