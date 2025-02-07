namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class AsteroidSpawnOutcome
{
    public bool Success { get; set; }
    public ulong? AsteroidId { get; set; }
    public int ResultSeed { get; set; }

    public static AsteroidSpawnOutcome Spawned(int seed, ulong asteroidId) 
        => new() { Success = true, AsteroidId = asteroidId, ResultSeed = seed };
}