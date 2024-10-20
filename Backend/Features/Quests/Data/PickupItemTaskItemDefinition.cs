using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
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
        var playerQuestRepository = context.Provider.GetRequiredService<IPlayerQuestRepository>();
        var questItem = await playerQuestRepository.GetAsync(context.QuestTaskId.QuestId);

        if (questItem == null)
        {
            return QuestInteractionOutcome.Failed($"Quest not found {context.QuestTaskId.QuestId}");
        }
        
        var factionRepository = context.Provider.GetRequiredService<IFactionRepository>();
        var factionItem = await factionRepository.FindAsync(questItem.FactionId);

        if (factionItem == null)
        {
            return QuestInteractionOutcome.Failed($"Faction not found {questItem.FactionId.Id}");
        }
        
        var itemSpawner = context.Provider.GetRequiredService<IItemSpawnerService>();

        const string pveModBaseUrl = "@{PVE_MOD}"; 
        var questTaskId = context.QuestTaskId;

        await itemSpawner.GiveTakeItemsWithCallback(
            new GiveTakePlayerItemsWithCallbackCommand(
                context.PlayerId,
                Items,
                new EntityId { organizationId = factionItem.OrganizationId ?? 0 },
                new Dictionary<string, PropertyValue>
                {
                    { "missionId", new PropertyValue(questItem.Seed) },
                    { "questId", new PropertyValue($"{questTaskId.QuestId.Id}") },
                    { "questTaskId", new PropertyValue($"{questTaskId.Id}") },
                },
                $"{pveModBaseUrl}/quest/callback/{questTaskId.QuestId.Id}/task/{questTaskId.Id}/complete",
                $"{pveModBaseUrl}/quest/callback/{questTaskId.QuestId.Id}/task/{questTaskId.Id}/failed"
            )
        );

        return QuestInteractionOutcome.Successful("Request to pickup items sent to Orleans");
    }
}