using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public class QuestInteractionService(IServiceProvider provider) : IQuestInteractionService
{
    public async Task<QuestInteractionOutcomeCollection> InteractAsync(QuestInteractCommand command)
    {
        var playerQuestRepository = provider.GetRequiredService<IPlayerQuestRepository>();
        var playerQuestItems = (await playerQuestRepository
                .GetAllByStatusAsync(command.PlayerId, QuestStatus.InProgress)).ToList();

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
        }

        return new QuestInteractionOutcomeCollection(
            interactionOutcomeList
        );
    }

    public async Task<QuestTaskCompletionOutcome> CompleteTaskAsync(QuestTaskId questTaskId)
    {
        var playerQuestRepository = provider.GetRequiredService<IPlayerQuestRepository>();
        await playerQuestRepository.CompleteTaskAsync(questTaskId);
        
        return QuestTaskCompletionOutcome.Completed();
    }
}