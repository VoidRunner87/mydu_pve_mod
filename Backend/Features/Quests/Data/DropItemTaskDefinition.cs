using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class DropItemTaskDefinition(
    TerritoryContainerItem container,
    IEnumerable<ElementQuantityRef> deliveryItems
) : TransportItemTaskDefinition(container, deliveryItems)
{
    public override bool IsMatchedBy(QuestInteractionContext context)
    {
        return Container.ConstructId == context.ConstructId;
    }

    public override async Task<QuestInteractionOutcome> HandleInteractionAsync(QuestInteractionContext context)
    {
        await Task.Yield();

        return QuestInteractionOutcome.Successful("");
    }
}