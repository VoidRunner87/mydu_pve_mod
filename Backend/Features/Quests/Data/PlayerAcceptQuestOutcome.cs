using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class PlayerAcceptQuestOutcome : IOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public static PlayerAcceptQuestOutcome Accepted() => new() {Success = true};
    public static PlayerAcceptQuestOutcome AlreadyAccepted() => new() {Success = false, Message = "Quest was already accepted"};
    public static PlayerAcceptQuestOutcome MaxNumberOfActiveQuestsReached() => new() {Success = false, Message = "You reached the maximum number of quests you can accept."};
    public static PlayerAcceptQuestOutcome Failed(string message) => new() { Message = message };
}