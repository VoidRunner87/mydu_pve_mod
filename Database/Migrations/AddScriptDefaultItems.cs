using System;
using FluentMigrator;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(9)]
public class AddScriptDefaultItems : Migration
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
                name = "spawn-poi-easy-encounter",
                content = JsonConvert.SerializeObject(
                    new ScriptActionItem
                    {
                        Name = "spawn-poi-easy-encounter",
                        Area = new ScriptActionAreaItem(),
                        Type = "spawn",
                        Prefab = "wreck-unknown-origin",
                        MinQuantity = 1,
                        MaxQuantity = 1
                    }
                )
            });

        Insert.IntoTable(ScriptTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "spawn-poi-medium-encounter",
                content = JsonConvert.SerializeObject(
                    new ScriptActionItem
                    {
                        Name = "spawn-poi-medium-encounter",
                        Area = new ScriptActionAreaItem(),
                        Type = "spawn",
                        Prefab = "wreck-unknown-origin",
                        MinQuantity = 1,
                        MaxQuantity = 1
                    }
                )
            });

        Insert.IntoTable(ScriptTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "spawn-poi-hard-encounter",
                content = JsonConvert.SerializeObject(
                    new ScriptActionItem
                    {
                        Name = "spawn-poi-hard-encounter",
                        Area = new ScriptActionAreaItem(),
                        Type = "spawn",
                        Prefab = "wreck-unknown-origin",
                        MinQuantity = 1,
                        MaxQuantity = 1
                    }
                )
            });

        Insert.IntoTable(ScriptTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "spawn-easy-encounter",
                content = JsonConvert.SerializeObject(
                    new ScriptActionItem
                    {
                        Name = "spawn-easy-encounter",
                        Area = new ScriptActionAreaItem(),
                        Type = "spawn",
                        Prefab = "live-unknown-origin",
                        MinQuantity = 1,
                        MaxQuantity = 1
                    }
                )
            });

        Insert.IntoTable(ScriptTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "spawn-medium-encounter",
                content = JsonConvert.SerializeObject(
                    new ScriptActionItem
                    {
                        Name = "spawn-medium-encounter",
                        Area = new ScriptActionAreaItem(),
                        Type = "spawn",
                        Prefab = "live-unknown-origin",
                        MinQuantity = 1,
                        MaxQuantity = 1
                    }
                )
            });

        Insert.IntoTable(ScriptTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "spawn-hard-encounter",
                content = JsonConvert.SerializeObject(
                    new ScriptActionItem
                    {
                        Name = "spawn-hard-encounter",
                        Area = new ScriptActionAreaItem(),
                        Type = "spawn",
                        Prefab = "live-unknown-origin",
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
                name = "wreck-unknown-origin",
                content = JsonConvert.SerializeObject(
                    new ConstructDefinitionItem
                    {
                        Name = "wreck-unknown-origin",
                        Folder = "wrecks",
                        Path = "Wreck_5_UnknownOrigin.json",
                        OwnerId = 0,
                        ServerProperties = new ConstructDefinitionItem.ServerPropertiesItem
                        {
                            IsDynamicWreck = true,
                            Header = new ConstructDefinitionItem.ServerPropertiesItem.HeaderProp
                            {
                                PrettyName = "Unknown Ship Emissions"
                            }
                        }
                    }
                )
            });

        Insert.IntoTable(ConstructDefinitionTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "live-unknown-origin",
                content = JsonConvert.SerializeObject(
                    new ConstructDefinitionItem
                    {
                        Name = "live-unknown-origin",
                        Folder = "wrecks",
                        Path = "Wreck_5_UnknownOrigin.json",
                        OwnerId = 4,
                        ServerProperties = new ConstructDefinitionItem.ServerPropertiesItem
                        {
                            IsDynamicWreck = true,
                            Header = new ConstructDefinitionItem.ServerPropertiesItem.HeaderProp
                            {
                                PrettyName = "UNKNOWN"
                            }
                        }
                    }
                )
            });
    }

    public override void Down()
    {
        Execute.Sql($"DELETE FROM {ScriptTable}");
    }
}