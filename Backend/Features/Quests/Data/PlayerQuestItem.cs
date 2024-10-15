using System;
using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Faction.Data;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class PlayerQuestItem(
    Guid id,
    FactionId factionId,
    ulong playerId,
    string type,
    int seed,
    PlayerQuestItem.QuestProperties properties,
    DateTime createdAt,
    DateTime? expiresAt,
    IEnumerable<QuestTaskItem> taskItems)
{
    public Guid Id { get; } = id;
    public FactionId FactionId { get; } = factionId;
    public ulong PlayerId { get; } = playerId;
    public string Type { get; } = type;
    public int Seed { get; } = seed;
    public QuestProperties Properties { get; } = properties;
    public DateTime CreatedAt { get; } = createdAt;
    public DateTime? ExpiresAt { get; } = expiresAt;
    public IEnumerable<QuestTaskItem> TaskItems { get; } = taskItems;

    public bool IsExpired(DateTime now)
    {
        return now > ExpiresAt;
    }

    public class QuestProperties(string title, string description)
    {
        public string Title { get; } = title;
        public string Description { get; } = description;
    }
}