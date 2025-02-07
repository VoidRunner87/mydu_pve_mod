using System;
using System.Text;
using System.Threading.Tasks;
using Backend;
using Backend.Database;
using Backend.Fixture;
using Backend.Fixture.Construct;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.Spawner.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json.Linq;
using NQ.Interfaces;
using NQutils.Sql;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Services;

public class AsteroidSpawnerService : IAsteroidSpawnerService
{
    private readonly Random _random = ModBase.ServiceProvider.GetRandomProvider().GetRandom();
    private readonly IUserContent _userContent = ModBase.ServiceProvider.GetRequiredService<IUserContent>();
    private readonly ISql _sql = ModBase.ServiceProvider.GetRequiredService<ISql>();
    private readonly IVoxelService _voxelService = ModBase.ServiceProvider.GetRequiredService<IVoxelService>();
    private readonly IGameplayBank _bank = ModBase.ServiceProvider.GetGameplayBank();
    private readonly IRDMSStorage _rdmsStorage = ModBase.ServiceProvider.GetRequiredService<IRDMSStorage>();
    private readonly IPlanetList _planetList = ModBase.ServiceProvider.GetRequiredService<IPlanetList>();
    private readonly IClusterClient _orleans = ModBase.ServiceProvider.GetOrleans();

    public async Task<ulong> SpawnAsteroid(SpawnAsteroidCommand command)
    {
        using var source = FixtureSource.FromStringContent(command.Data.ToString(), FixtureKind.Construct);
        
        var fixture = ConstructFixture.FromSource(source);
        fixture.position = command.Position;
        fixture.header.constructIdHint = new ulong?();
        fixture.header.prettyName = GenerateName(command.Prefix);
        fixture.planet.planetProperties.description.displayName = fixture.header.prettyName;
        fixture.parentId = new ulong?();
        fixture.serverProperties.dynamicFixture = true;
        
        var cid = (await ConstructFixtureImport.Import(
            fixture,
            _userContent,
            _sql,
            _voxelService,
            _bank,
            _rdmsStorage,
            _planetList,
            ConstructSqlExtension.InsertMode.DynamicAsteroid
        ))[0];
        
        await _orleans.GetConstructParentingGrain().SpawnConstruct(cid);

        if (command.RegisterAsteroid)
        {
            await _sql.AsteroidRegister(
                cid,
                command.Tier,
                command.Planet,
                fixture.planet.planetProperties.altitudeReferenceRadius,
                (long)fixture.size
            );
        }

        return cid;
    }

    public async Task<AsteroidSpawnOutcome> SpawnAsteroidWithData(SpawnAsteroidCommand command)
    {
        command.Tier = Math.Clamp(command.Tier, 1, 5);
        command.Radius = Math.Clamp(command.Radius, 32, 2048);
        
        var asteroidJToken = AsteroidData.GetBase();

        var randomSeed = _random.Next(); 
        //Size 32 Seeds: 418819536, 1814325084, 1799286799, 201633054
        ReplaceSeedValues(command.Data, randomSeed);
        
        var jsonString = command.Data.ToString();
        var jsonBytes = Encoding.UTF8.GetBytes(jsonString);
        var jsonData = Lz4CompressionService.PrependDecompressedSize(jsonBytes.Length, Lz4CompressionService.Compress(jsonString));
        
        asteroidJToken["pipeline"] = jsonData;
        asteroidJToken["voxelGeometry"]!["maxRadius"] = command.Radius;
        asteroidJToken["voxelGeometry"]!["radius"] = command.Radius;
        asteroidJToken["planetProperties"]!["altitudeReferenceRadius"] = command.Radius;
        asteroidJToken["planetProperties"]!["seaLevelGravity"] = command.Gravity;
        asteroidJToken["rotation"] = JToken.FromObject(_random.RandomQuaternion().ToNqQuat());
        // asteroidJToken["ores"] = JArray.FromObject(command.Ores);

        command.Data = asteroidJToken;
        
        var asteroidId = await SpawnAsteroid(command);

        return AsteroidSpawnOutcome.Spawned(randomSeed, asteroidId);
    }

    private static void ReplaceSeedValues(JToken token, object newValue)
    {
        var seeds = token.SelectTokens("$..seed");
        foreach (var seed in seeds)
        {
            seed.Replace(JToken.FromObject(newValue));
        }
    }

    private string GenerateName(string prefix)
    {
        var str = $"{prefix}-";
        for (var index = 0; index < 3; ++index)
            str += ((char)(65 + _random.Next(26))).ToString();
        var name = str + "-";
        for (var index = 0; index < 3; ++index)
            name += ((char)(48 + _random.Next(10))).ToString();
        return name;
    }
}