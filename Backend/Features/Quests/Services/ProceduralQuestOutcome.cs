using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Data;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public class ProceduralQuestOutcome : IOutcome
{
    public bool Success { get; private init; }
    public string Message { get; private init; }
    public ProceduralQuestItem QuestItem { get; private init; }

    public static ProceduralQuestOutcome Created(ProceduralQuestItem questItem)
        => new() { QuestItem = questItem, Success = true };

    public static ProceduralQuestOutcome Failed(string message)
        => new() { Success = false, Message = message };
}