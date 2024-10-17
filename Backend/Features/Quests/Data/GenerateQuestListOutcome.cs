using System.Collections.Generic;
using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class GenerateQuestListOutcome : IOutcome
{
    public IEnumerable<ProceduralQuestItem> QuestList { get; set; } = [];
    public string Message { get; set; } = string.Empty;

    public static GenerateQuestListOutcome NoQuestsAvailable(string message) => new(){Message = message};

    public static GenerateQuestListOutcome WithAvailableQuests(IEnumerable<ProceduralQuestItem> items) =>
        new()
        {
            QuestList = items
        };
}