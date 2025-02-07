using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Quests.Data;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public interface IQuestTaskItemDefinition
{
    public IEnumerable<QuestElementQuantityRef> Items { get; set; }

    bool IsMatchedBy(QuestInteractionContext context);
    Task<QuestInteractionOutcome> HandleInteractionAsync(QuestInteractionContext context);
    IQuestTaskCompletionHandler GetCompletionHandler(QuestCompletionHandlerContext context);
}