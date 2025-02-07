using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestTaskCompletionOutcome : IOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public static QuestTaskCompletionOutcome Completed() => new() { Success = true };

    public static QuestTaskCompletionOutcome CompletedButFailedToHandleCompletion(string message) => new()
        { Success = true, Message = $"Completed Quest but failed to handle completion. {message}" };
    public static QuestTaskCompletionOutcome NotFound() => new() { Success = false, Message = "Not Found" };
    public static QuestTaskCompletionOutcome Failed(string message) => new() { Success = true, Message = message };
}