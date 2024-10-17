using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Quests.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public abstract class TransportItemTaskDefinition(TerritoryContainerItem container) : IQuestTaskItemDefinition
{
    public TerritoryContainerItem Container { get; set; } = container;
    public virtual bool IsMatchedBy(QuestInteractionContext context)
    {
        return false;
    }

    public virtual Task<QuestInteractionOutcome> HandleInteractionAsync(QuestInteractionContext context)
    {
        return Task.FromResult(QuestInteractionOutcome.Failed("Not implemented"));
    }
}