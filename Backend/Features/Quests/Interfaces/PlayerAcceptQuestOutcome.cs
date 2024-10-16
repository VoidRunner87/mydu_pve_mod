using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public class PlayerAcceptQuestOutcome : IOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public static PlayerAcceptQuestOutcome Accepted() => new() {Success = true};
    public static PlayerAcceptQuestOutcome Failed(string message) => new() { Message = message };
}