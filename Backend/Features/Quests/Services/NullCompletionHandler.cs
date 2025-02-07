using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public class NullCompletionHandler : IQuestTaskCompletionHandler
{
    public Task<QuestTaskCompletionHandlerOutcome> HandleCompletion()
    {
        return Task.FromResult(QuestTaskCompletionHandlerOutcome.Handled());
    }
}