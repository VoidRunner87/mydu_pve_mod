using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Services;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public abstract class TransportItemTaskDefinition(
    TerritoryContainerItem container,
    IEnumerable<QuestElementQuantityRef> items
) : IQuestTaskItemDefinition
{
    public TerritoryContainerItem Container { get; } = container;
    public IEnumerable<QuestElementQuantityRef> Items { get; set; } = items;

    public abstract bool IsMatchedBy(QuestInteractionContext context);

    public abstract Task<QuestInteractionOutcome> HandleInteractionAsync(QuestInteractionContext context);
    public virtual IQuestTaskCompletionHandler GetCompletionHandler(QuestCompletionHandlerContext context) => new NullCompletionHandler();
}