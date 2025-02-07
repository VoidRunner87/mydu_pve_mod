using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Features.Faction.Data;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestInteractionOutcome : IOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public QuestTaskId QuestTaskId { get; set; }

    public static QuestInteractionOutcome Failed(string message)
        => new() { Success = false, Message = message };

    public static QuestInteractionOutcome QuestNotFound(QuestId questId)
        => Failed($"Quest not found {questId}");

    public static QuestInteractionOutcome FactionNotFound(FactionId factionId)
        => Failed($"Faction not found {factionId}");

    public static QuestInteractionOutcome Successful(QuestTaskId questTaskId, string message)
        => new() { QuestTaskId = questTaskId, Success = true, Message = message };
}