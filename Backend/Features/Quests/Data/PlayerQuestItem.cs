using System;
using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class PlayerQuestItem(
    QuestId id,
    Guid originalQuestId,
    FactionId factionId,
    ulong playerId,
    string type,
    string status,
    int seed,
    PlayerQuestItem.QuestProperties properties,
    DateTime createdAt,
    DateTime? expiresAt,
    IList<QuestTaskItem> taskItems,
    ScriptActionItem onSuccessScript,
    ScriptActionItem onFailureScript
)
{
    public Guid Id { get; } = id;
    public Guid OriginalQuestId { get; } = originalQuestId;
    public FactionId FactionId { get; } = factionId;
    public ulong PlayerId { get; } = playerId;
    public string Type { get; } = type;
    public string Status { get; } = status;
    public int Seed { get; } = seed;
    public QuestProperties Properties { get; } = properties;
    public DateTime CreatedAt { get; } = createdAt;
    public DateTime? ExpiresAt { get; } = expiresAt;
    public IList<QuestTaskItem> TaskItems { get; } = taskItems;
    public ScriptActionItem OnSuccessScript { get; } = onSuccessScript;
    public ScriptActionItem OnFailureScript { get; } = onFailureScript;

    public bool IsExpired(DateTime now) => now > ExpiresAt;

    public QuestTaskItem? GetTaskOrNull(QuestTaskId questTaskId)
    {
        return TaskItems.SingleOrDefault(q => q.Id.Equals(questTaskId));
    }

    public class QuestProperties(string title, string description)
    {
        public string Title { get; set; } = title;
        public string Description { get; set; } = description;
        public IEnumerable<string> RewardTextList { get; set; } = [];
        public long QuantaReward { get; set; } = 0;
        public Dictionary<long, long> InfluenceReward { get; set; } = [];
        public Dictionary<string, long> ItemRewardMap { get; set; } = [];
        public double DistanceSu { get; set; } = 0;
        public double DistanceMeters { get; set; } = 0;
    }
}