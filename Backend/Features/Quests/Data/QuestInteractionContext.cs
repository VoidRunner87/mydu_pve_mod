using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestInteractionContext
{
    /// <summary>
    /// Player that is performing the interaction
    /// </summary>
    public PlayerId PlayerId { get; set; }
    /// <summary>
    /// The construct being interacted with
    /// </summary>
    public ConstructId? ConstructId { get; set; }
    /// <summary>
    /// Element in the construct being interacted with
    /// </summary>
    public ElementId? ElementId { get; set; }
}