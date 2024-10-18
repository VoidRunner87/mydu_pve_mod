using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public abstract class TransportItemTaskDefinition(
    TerritoryContainerItem container,
    IEnumerable<ElementQuantityRef> items
) : IQuestTaskItemDefinition
{
    public TerritoryContainerItem Container { get; } = container;
    public IEnumerable<ElementQuantityRef> Items { get; set; } = items;
    
    public abstract bool IsMatchedBy(QuestInteractionContext context);

    public abstract Task<QuestInteractionOutcome> HandleInteractionAsync(QuestInteractionContext context);
}