using System;
using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class ProceduralQuestItem(
    Guid id,
    FactionId factionId,
    string type,
    int seed,
    string title,
    ProceduralQuestProperties properties,
    IEnumerable<QuestTaskItem> taskItems
)
{
    public Guid Id { get; } = id;
    public FactionId FactionId { get; } = factionId;
    public string Type { get; } = type;
    public int Seed { get; } = seed;
    public string Title { get; } = title;
    public ProceduralQuestProperties Properties { get; } = properties;
    public IEnumerable<QuestTaskItem> TaskItems { get; } = taskItems;
    public double Distance => CalculateTotalDistance();

    public double CalculateTotalDistance()
    {
        var tasks = TaskItems.ToList();

        if (tasks.Count < 2)
        {
            return 0;
        }

        double totalDistance = 0;
        QuestTaskItem? lastTask = null;
        
        foreach (var task in tasks)
        {
            if (lastTask != null)
            {
                totalDistance += (task.Position - lastTask.Position).Size();
            }

            lastTask = task;
        }

        return totalDistance;
    }
}