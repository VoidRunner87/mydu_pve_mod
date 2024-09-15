using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("starter")]
public class StarterContentController(IServiceProvider provider) : Controller
{
    [SwaggerOperation("Initializes Starter Content")]
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> InstallStarterContent()
    {
        var config = NQutils.Config.Config.Instance;
        var dataPath = config.s3.override_base_path;

        if (!Directory.Exists(dataPath))
        {
            return BadRequest($"Can't find path: {dataPath}");
        }

        var pveDirectoryPath = Path.Combine(dataPath, "pve");
        if (!Directory.Exists(pveDirectoryPath))
        {
            Directory.CreateDirectory(pveDirectoryPath);
        }

        const string simplePoiFileName = "Simple_POI.json";
        const string basicPirateFileName = "Basic_Pirate.json";

        var poiContents = ResourceLoader.GetContents($"Mod.DynamicEncounters.Resources.{simplePoiFileName}");
        var basicPirateContents = ResourceLoader.GetContents($"Mod.DynamicEncounters.Resources.{basicPirateFileName}");
        
        var simplePoiDestinationPath = Path.Combine(pveDirectoryPath, simplePoiFileName);
        var basicPirateDestinationPath = Path.Combine(pveDirectoryPath, basicPirateFileName);

        if (!System.IO.File.Exists(simplePoiDestinationPath))
        {
            await using var poiSw = System.IO.File.CreateText(simplePoiDestinationPath);
            await poiSw.WriteLineAsync(poiContents);
        }

        if (!System.IO.File.Exists(simplePoiDestinationPath))
        {
            await using var basicPirateSw = System.IO.File.CreateText(basicPirateContents);
            await basicPirateSw.WriteAsync(basicPirateDestinationPath);
        }

        var prefabItemRepository = provider.GetRequiredService<IPrefabItemRepository>();
        var scriptItemRepository = provider.GetRequiredService<IScriptActionItemRepository>();
        var sectorEncounterRepository = provider.GetRequiredService<ISectorEncounterRepository>();
        var featureService = provider.GetRequiredService<IFeatureWriterService>();

        var poiPrefabGuid = Guid.Parse("cb154c88-ca02-47d6-8e52-0676d419a0e9");
        var piratePrefabGuid = Guid.Parse("d76221e2-a81f-4d04-898d-774404f96004");
        var sectorEncounterGuid = piratePrefabGuid;

        var allPrefabs = (await prefabItemRepository.GetAllAsync())
            .Select(x => x.Id).ToHashSet();

        if (!allPrefabs.Contains(poiPrefabGuid))
        {
            await prefabItemRepository.AddAsync(
                new PrefabItem
                {
                    Id = poiPrefabGuid,
                    Name = "basic-poi",
                    Folder = "pve",
                    Path = "Simple_POI.json",
                    OwnerId = 0,
                    ServerProperties = new PrefabItem.ServerPropertiesItem
                    {
                        Header = new PrefabItem.ServerPropertiesItem.HeaderProp
                        {
                            PrettyName = "[0] POI"
                        },
                        IsDynamicWreck = true
                    }
                }
            );
        }

        if (!allPrefabs.Contains(piratePrefabGuid))
        {
            await prefabItemRepository.AddAsync(
                new PrefabItem
                {
                    Id = piratePrefabGuid,
                    Name = "basic-pirate",
                    Folder = "pve",
                    Path = "Basic_Pirate.json",
                    OwnerId = 4,
                    AccelerationG = 5,
                    InitialBehaviors =
                    [
                        "aggressive",
                        "follow-target"
                    ],
                    WeaponItems = ["WeaponCannonExtraSmallAgile3"],
                    AmmoItems =
                    [
                        "AmmoCannonExtraSmallThermicUncommon",
                        "AmmoCannonExtraSmallKineticUncommon"
                    ],
                    ServerProperties = new PrefabItem.ServerPropertiesItem
                    {
                        Header = new PrefabItem.ServerPropertiesItem.HeaderProp
                        {
                            PrettyName = "Lone Pirate"
                        },
                        IsDynamicWreck = false
                    }
                }
            );
        }

        var scriptsSet = (await scriptItemRepository.GetAllAsync())
            .Select(x => x.Name)
            .ToHashSet();

        if (!scriptsSet.Contains("spawn-basic-poi"))
        {
            await scriptItemRepository.AddAsync(
                new ScriptActionItem
                {
                    Name = "spawn-basic-poi",
                    Actions =
                    [
                        new()
                        {
                            Type = "spawn",
                            Prefab = "basic-poi",
                            Tags = ["poi"],
                            Override = new ScriptActionOverrides
                            {
                                ConstructName = "[0] Pirate Attack"
                            }
                        }
                    ]
                }
            );
        }

        if (!scriptsSet.Contains("spawn-basic-pirate"))
        {
            await scriptItemRepository.AddAsync(
                new ScriptActionItem
                {
                    Name = "spawn-basic-pirate",
                    Actions =
                    [
                        new()
                        {
                            Type = "spawn",
                            Prefab = "basic-pirate"
                        },
                        new()
                        {
                            Type = "expire-sector"
                        },
                        new()
                        {
                            Type = "for-each-handle-with-tag",
                            Tags = ["poi"],
                            Actions =
                            [
                                new ScriptActionItem
                                {
                                    Type = "delete"
                                }
                            ]
                        }
                    ]
                }
            );
        }

        if (!scriptsSet.Contains("expire-sector-default"))
        {
            await scriptItemRepository.AddAsync(
                new ScriptActionItem
                {
                    Name = "expire-sector-default",
                    Actions =
                    [
                        new()
                        {
                            Type = "expire-sector",
                        },
                        new()
                        {
                            Type = "publish-wreck-discovered-event"
                        }
                    ]
                }
            );
        }

        var sectorEncounterSet = (await sectorEncounterRepository.GetAllAsync())
            .Select(x => x.Id)
            .ToHashSet();

        if (!sectorEncounterSet.Contains(sectorEncounterGuid))
        {
            await sectorEncounterRepository.AddAsync(
                new SectorEncounterItem
                {
                    Id = sectorEncounterGuid,
                    Name = "Basic Pirate Encounter",
                    Active = true,
                    OnLoadScript = "spawn-basic-poi",
                    OnSectorEnterScript = "spawn-basic-pirate"
                }
            );
        }

        await featureService.EnableStarterContentFeaturesAsync();

        return Ok("Starter Content Added. Features Enabled. Have FUN!");
    }
}