using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class PickupItemTaskItemDefinition(
    TerritoryContainerItem container,
    IEnumerable<ElementQuantityRef> pickupItems
) : TransportItemTaskDefinition(container, pickupItems)
{
    public override bool IsMatchedBy(QuestInteractionContext context)
    {
        return context.ConstructId == Container.ConstructId;
    }

    public override async Task<QuestInteractionOutcome> HandleInteractionAsync(QuestInteractionContext context)
    {
        var itemSpawner = context.Provider.GetRequiredService<IItemSpawnerService>();

        var playerAlertService = context.Provider.GetRequiredService<IPlayerAlertService>();
        await playerAlertService.SendInfoAlert(context.PlayerId, "Mission items picked up");
        await itemSpawner.SpawnItems(
            new SpawnItemsOnPlayerInventoryCommand(
                context.PlayerId,
                Items,
                new Dictionary<string, PropertyValue>
                {
                    {"questId", new PropertyValue($"{context.QuestTaskId.QuestId.Id}")}
                }
            )
        );

        return QuestInteractionOutcome.Successful("Mission items picked up");
    }
}