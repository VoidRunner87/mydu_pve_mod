using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public class PlayerAbandonQuestOutcome : IOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public static PlayerAbandonQuestOutcome Abandoned() => new() {Success = true};
    public static PlayerAbandonQuestOutcome Failed(string message) => new() { Message = message };
}