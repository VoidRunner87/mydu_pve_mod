using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestTaskCompletionHandlerOutcome : IOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static QuestTaskCompletionHandlerOutcome Handled() => new() { Success = true };
    
    public static QuestTaskCompletionHandlerOutcome QuestTaskNotFound(QuestTaskId questTaskId)
        => new() { Success = false, Message = $"Quest Task {questTaskId} Not Found"};
}