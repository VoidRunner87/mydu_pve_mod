using System;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestCompletionHandlerContext(
    IServiceProvider provider,
    QuestTaskId questTaskId
)
{
    public IServiceProvider Provider { get; } = provider;

    /// <summary>
    /// Quest Task Id
    /// </summary>
    public QuestTaskId QuestTaskId { get; } = questTaskId;
}