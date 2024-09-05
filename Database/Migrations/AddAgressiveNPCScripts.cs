using System;
using FluentMigrator;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(15)]
public class AddAgressiveNPCScripts : Migration
{
    private const string ScriptTable = "mod_script";
    private const string ConstructDefinitionTable = "mod_construct_def";
    
    public override void Up()
    {
        Insert.IntoTable(ScriptTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "spawn-test-encounter",
                content = JsonConvert.SerializeObject(
                    new ScriptActionItem
                    {
                        Name = "spawn-test-encounter",
                        Area = new ScriptActionAreaItem(),
                        Type = "spawn",
                        Prefab = "test-enemy",
                        MinQuantity = 1,
                        MaxQuantity = 1
                    }
                )
            });
        
        Insert.IntoTable(ConstructDefinitionTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "test-enemy",
                content = JsonConvert.SerializeObject(
                    new PrefabItem
                    {
                        Name = "test-enemy",
                        Folder = "wrecks",
                        Path = "Wreck_5_UnknownOrigin.json",
                        OwnerId = 4,
                        InitialBehaviors =
                        [
                            "aggressive",
                            "follow-target"
                        ],
                        ServerProperties = new PrefabItem.ServerPropertiesItem
                        {
                            IsDynamicWreck = false,
                            Header = new PrefabItem.ServerPropertiesItem.HeaderProp
                            {
                                PrettyName = "TEST ENEMY"
                            }
                        }
                    }
                )
            });
    }

    public override void Down()
    {
        
    }
}