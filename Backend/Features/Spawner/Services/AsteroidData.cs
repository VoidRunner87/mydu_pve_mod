using System;
using Newtonsoft.Json.Linq;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Services;

public static class AsteroidData
{
    public const int MinRadius = 16;
    public const int MaxRadius = 2048;

    public static JToken GetBase()
    {
        return JToken.FromObject(new
        {
            header = new
            {
                biomeEditorGitRevision = "00dfc8b1a408ac880f9d4f2aaa165e1b552bc2ee",
                biomeEditorVersion = "3",
                constructIdHint = 999,
                prettyName = "",
                uniqueIdentifier = "basic_5_13",
            },
            kind = "asteroid",
            pipeline = "",
            planetProperties = new
            {
                altitudeReferenceRadius = 500D,
                decorAssetList = Array.Empty<object>(),
                description = new
                {
                    biosphere = "",
                    classification = "",
                    discoveredBy = "",
                    displayName = "",
                    habitabilityClass = "",
                    information = "",
                    numSatellites = 0,
                    positionFromSun = 0,
                    type = ""
                },
                isSanctuary = false,
                isTutorial = false,
                materialsAssetList = (string[]) ["CraterRaw"],
                maxGenerationRadiusHint = 0D,
                minGenerationRadiusHint = 0D,
                ores = (string[]) ["ChromiumOre"],
                seaLevelGravity = 1D,
                territoryTileSize = 500D,
            },
            position = new Vec3(),
            rotation = Quat.Identity,
            serializationVersion = 1,
            size = 2048,
            voxelGeometry = new
            {
                kind = "Octree",
                maxRadius = 750D,
                radius = 500D,
                size = 2048,
                voxelLod0 = 5
            }
        });
    }
}