using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public interface IItemSpawnerService
{
    Task SpawnItems(SpawnItemCommand command);
}