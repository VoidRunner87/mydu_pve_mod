using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Quests.Data;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public interface IQuestTaskFactory
{
    Task<QuestTaskItem> CreatePickupItemTask(
        ulong constructId, 
        string elementTypeName, 
        long quantity
    );
}