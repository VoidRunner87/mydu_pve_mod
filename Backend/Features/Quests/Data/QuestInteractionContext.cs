using System;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestInteractionContext(
    IServiceProvider provider,
    PlayerId playerId, 
    ConstructId? constructId, 
    ElementId? elementId,
    QuestTaskId questTaskId
)
{
    public IServiceProvider Provider { get; } = provider;

    /// <summary>
    /// Player that is performing the interaction
    /// </summary>
    public PlayerId PlayerId { get; } = playerId;

    /// <summary>
    /// The construct being interacted with
    /// </summary>
    public ConstructId? ConstructId { get; } = constructId;

    /// <summary>
    /// Element in the construct being interacted with
    /// </summary>
    public ElementId? ElementId { get; } = elementId;

    /// <summary>
    /// Quest Task Id
    /// </summary>
    public QuestTaskId QuestTaskId { get; } = questTaskId;
}