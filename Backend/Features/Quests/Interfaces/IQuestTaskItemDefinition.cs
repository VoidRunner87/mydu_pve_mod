using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Quests.Data;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public interface IQuestTaskItemDefinition
{
    bool IsMatchedBy(QuestInteractionContext context);
    Task<QuestInteractionOutcome> HandleInteractionAsync(QuestInteractionContext context);
}