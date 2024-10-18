using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestInteractCommand
{
    public PlayerId PlayerId { get; set; }
    public ConstructId ConstructId { get; set; }
    public ElementId? ElementId { get; set; }
}