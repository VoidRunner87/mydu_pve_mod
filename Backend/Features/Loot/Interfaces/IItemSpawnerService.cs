using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public interface IItemSpawnerService
{
    Task SpawnItems(SpawnItemOnRandomContainersCommand command);
    Task SpawnItemsForPlayersAround(SpawnItemOnRandomContainersAroundAreaCommand command);
    Task GiveTakeItemsWithCallback(GiveTakePlayerItemsWithCallbackCommand command);
    Task SpawnSpaceFuel(SpawnFuelCommand command);
}