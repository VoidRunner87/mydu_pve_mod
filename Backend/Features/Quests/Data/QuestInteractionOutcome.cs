using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestInteractionOutcome : IOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public static QuestInteractionOutcome Failed(string message) => new() { Success = false, Message = message };
    public static QuestInteractionOutcome Successful(string message) => new() { Success = true, Message = message };
}