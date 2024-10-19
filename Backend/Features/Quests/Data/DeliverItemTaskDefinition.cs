using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class DeliverItemTaskDefinition(
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
        var itemSpawner = context.Provider.GetRequiredService<IItemSpawnerService>();
        
        const string pveModBaseUrl = "@{PVE_MOD}"; 
        var questTaskId = context.QuestTaskId;
        
        await itemSpawner.SpawnItemsWithCallback(
            new GiveTakePlayerItemsWithCallbackCommand(
                context.PlayerId,
                Items,
                new Dictionary<string, PropertyValue>
                {
                    {"questId", new PropertyValue($"{questTaskId.QuestId.Id}")},
                    {"questTaskId", new PropertyValue($"{questTaskId.Id}")},
                },
                $"{pveModBaseUrl}/quest/callback/{questTaskId.QuestId.Id}/task/{questTaskId.Id}/complete",
                $"{pveModBaseUrl}/quest/callback/{questTaskId.QuestId.Id}/task/{questTaskId.Id}/failed"
            )
        );

        return QuestInteractionOutcome.Successful("Request to Deliver items sent to Orleans");
    }
}