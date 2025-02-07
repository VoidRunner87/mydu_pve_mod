using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestInteractionOutcomeCollection(
    IEnumerable<QuestInteractionOutcome> outcomes
) : IOutcome
{
    public IEnumerable<QuestInteractionOutcome> Outcomes { get; } = outcomes;
    public bool Success => Outcomes.All(x => x.Success);
}