using System;
using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Faction.Data;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class ProceduralQuestItem(
    Guid id,
    FactionId factionId,
    string type,
    int seed,
    string title,
    ProceduralQuestProperties properties,
    IEnumerable<QuestTaskItem> taskItems
)
{
    public Guid Id { get; } = id;
    public FactionId FactionId { get; } = factionId;
    public string Type { get; } = type;
    public int Seed { get; } = seed;
    public string Title { get; } = title;
    public ProceduralQuestProperties Properties { get; } = properties;
    public IEnumerable<QuestTaskItem> TaskItems { get; } = taskItems;
}