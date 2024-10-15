using System;
using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Faction.Data;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class ProceduralQuestItem(
    Guid id,
    FactionId factionId,
    string type,
    int seed,
    IEnumerable<QuestTaskItem> taskItems
)
{
    public Guid Id { get; } = id;
    public FactionId FactionId { get; } = factionId;
    public string Type { get; } = type;
    public int Seed { get; } = seed;
    public IEnumerable<QuestTaskItem> TaskItems { get; } = taskItems;
}