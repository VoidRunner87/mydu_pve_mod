using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public interface IItemConsumerService
{
    Task ConsumeItems(ConsumeItemsOnPlayerInventoryCommand command);
}