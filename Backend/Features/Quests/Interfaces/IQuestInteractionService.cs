using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Quests.Data;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public interface IQuestInteractionService
{
    Task<QuestInteractionOutcomeCollection> InteractAsync(QuestInteractCommand command);
}