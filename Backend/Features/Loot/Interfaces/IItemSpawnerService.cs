using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public interface IItemSpawnerService
{
    Task SpawnItems(SpawnItemOnRandomContainersCommand onRandomContainersCommand);
    Task SpawnItems(SpawnItemsOnPlayerInventoryCommand command);
    Task SpawnFuel(SpawnFuelCommand command);
}