using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public class QuestInteractionService(IServiceProvider provider) : IQuestInteractionService
{
    public async Task<QuestInteractionOutcomeCollection> InteractAsync(QuestInteractCommand command)
    {
        var playerQuestRepository = provider.GetRequiredService<IPlayerQuestRepository>();
        var playerQuestItems = (await playerQuestRepository
            .GetAllByStatusAsync(command.PlayerId, [QuestStatus.InProgress])).ToList();

        var interactionOutcomeList = new List<QuestInteractionOutcome>();

        foreach (var questItem in playerQuestItems)
        {
            foreach (var taskItem in questItem.TaskItems.Where(ti => !ti.IsCompleted()))
            {
                var context = new QuestInteractionContext(
                    provider,
                    command.PlayerId,
                    command.ConstructId,
                    command.ElementId,
                    taskItem.Id
                );

                if (taskItem.Definition.IsMatchedBy(context))
                {
                    var interactionOutcome = await taskItem.Definition.HandleInteractionAsync(context);
                    interactionOutcomeList.Add(interactionOutcome);
                }
            }

            if (questItem.Status != QuestStatus.Completed)
            {
                if (await playerQuestRepository.AreAllTasksCompleted(questItem.Id))
                {
                    await playerQuestRepository.SetStatusAsync(questItem.Id, QuestStatus.Completed);
                }
            }
        }

        return new QuestInteractionOutcomeCollection(
            interactionOutcomeList
        );
    }

    public async Task<QuestTaskCompletionOutcome> CompleteTaskAsync(QuestTaskId questTaskId)
    {
        var playerQuestRepository = provider.GetRequiredService<IPlayerQuestRepository>();

        var questItem = await playerQuestRepository.GetAsync(questTaskId.QuestId);

        if (questItem == null)
        {
            return QuestTaskCompletionOutcome.NotFound();
        }

        await playerQuestRepository.CompleteTaskAsync(questTaskId);

        if (questItem.Status != QuestStatus.Completed)
        {
            if (await playerQuestRepository.AreAllTasksCompleted(questTaskId.QuestId.Id))
            {
                await playerQuestRepository.SetStatusAsync(questTaskId.QuestId.Id, QuestStatus.Completed);

                await TriggerQuestCompletionEvents(questItem.PlayerId, questTaskId.QuestId);
            }
        }

        return QuestTaskCompletionOutcome.Completed();
    }

    private async Task TriggerQuestCompletionEvents(PlayerId playerId, QuestId questId)
    {
        var playerQuestRepository = provider.GetRequiredService<IPlayerQuestRepository>();
        var itemSpawner = provider.GetRequiredService<IItemSpawnerService>();
        var questItem = await playerQuestRepository.GetAsync(questId);

        if (questItem == null)
        {
            return;
        }

        if (questItem.Properties.ItemRewardMap.Count > 0)
        {
            await itemSpawner.GiveTakeItemsWithCallback(
                new GiveTakePlayerItemsWithCallbackCommand(
                    playerId,
                    questItem.Properties.ItemRewardMap.Select(
                        x => new ElementQuantityRef(
                            new ElementId(),
                            new ElementTypeName(x.Key),
                            x.Value
                        )
                    ),
                    new EntityId { playerId = playerId },
                    new Dictionary<string, PropertyValue>(),
                    string.Empty,
                    string.Empty
                )
            );
        }

        var taskQueueService = provider.GetRequiredService<ITaskQueueService>();
        await taskQueueService.EnqueueScript(
            new ScriptActionItem
            {
                Type = GiveQuantaToPlayer.ActionName,
                Value = questItem.Properties.QuantaReward,
                Properties =
                {
                    { "PlayerIds", new ulong[] { playerId } },
                    { "QuestId", questItem.Id },
                    { "Reason", "Quest Completion" }
                }
            },
            DateTime.UtcNow
        );

        await provider.GetRequiredService<IPlayerAlertService>()
            .SendInfoAlert(playerId, "Mission completed");
    }
}