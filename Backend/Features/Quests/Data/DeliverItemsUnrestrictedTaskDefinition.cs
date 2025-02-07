using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Services;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Data;

/// <summary>
/// This one is used to items that should not be quest specific.
/// The operation accepts any items
/// </summary>
/// <param name="container"></param>
/// <param name="deliveryItems"></param>
public class DeliverItemsUnrestrictedTaskDefinition(
    TerritoryContainerItem container,
    IEnumerable<QuestElementQuantityRef> deliveryItems
) : TransportItemTaskDefinition(container, deliveryItems)
{
    public override bool IsMatchedBy(QuestInteractionContext context)
    {
        return Container.ConstructId == context.ConstructId;
    }

    public override async Task<QuestInteractionOutcome> HandleInteractionAsync(QuestInteractionContext context)
    {
        var playerQuestRepository = context.Provider.GetRequiredService<IPlayerQuestRepository>();
        var questItem = await playerQuestRepository.GetAsync(context.QuestTaskId.QuestId);

        if (questItem == null)
        {
            return QuestInteractionOutcome.QuestNotFound(context.QuestTaskId.QuestId);
        }

        var factionRepository = context.Provider.GetRequiredService<IFactionRepository>();
        var factionItem = await factionRepository.FindAsync(questItem.FactionId);

        if (factionItem == null)
        {
            return QuestInteractionOutcome.FactionNotFound(questItem.FactionId.Id);
        }

        var itemSpawner = context.Provider.GetRequiredService<IItemSpawnerService>();

        const string pveModBaseUrl = "@{PVE_MOD}";
        var questTaskId = context.QuestTaskId;

        await itemSpawner.GiveTakeItemsWithCallback(
            new GiveTakePlayerItemsWithCallbackCommand(
                context.PlayerId,
                Items.Select(x => new ElementQuantityRef(0, x.ElementTypeName, x.Quantity)),
                new EntityId(),
                new Dictionary<string, PropertyValue>(),
                $"{pveModBaseUrl}/quest/callback/{questTaskId.QuestId.Id}/task/{questTaskId.Id}/complete",
                $"{pveModBaseUrl}/quest/callback/{questTaskId.QuestId.Id}/task/{questTaskId.Id}/failed"
            )
        );

        return QuestInteractionOutcome.Successful(questTaskId,
            $"{questTaskId.QuestId.Id}/{questTaskId.Id} Request to Deliver items sent to Orleans");
    }

    public override IQuestTaskCompletionHandler GetCompletionHandler(QuestCompletionHandlerContext context)
        => new CreateBotMarketOrderCompletionHandler(
            new QuestCompletionHandlerContext(
                context.Provider,
                context.QuestTaskId
            )
        );
}