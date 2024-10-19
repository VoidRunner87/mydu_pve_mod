using System;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public struct QuestTaskId(QuestId questId, Guid id)
{
    public QuestId QuestId { get; } = questId;
    public Guid Id { get; } = id;
}