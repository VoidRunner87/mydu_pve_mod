using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Interfaces;

public interface IAsteroidSpawnerService
{
    Task<ulong> SpawnAsteroid(SpawnAsteroidCommand command);
    Task<AsteroidSpawnOutcome> SpawnAsteroidWithData(SpawnAsteroidCommand command);
}