using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestLootCalculationResult
{
    public required IEnumerable<QuestElementQuantityRef> QuestItems { get; set; }
    public required double TotalPrice { get; set; }
}