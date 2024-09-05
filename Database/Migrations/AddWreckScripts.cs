using System;
using System.Linq;
using FluentMigrator;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(12)]
public class AddWreckScripts : Migration
{
    private const string ScriptTable = "mod_script";
    private const string ModSectorInstanceTable = "mod_sector_instance";
    private const string ConstructDefinitionTable = "mod_construct_def";

    public override void Up()
    {
        Alter.Table(ModSectorInstanceTable)
            .InSchema("public")
            .AlterColumn("on_load_script").AsString().Nullable()
            .AlterColumn("on_sector_enter_script").AsString().Nullable();
        
        Insert.IntoTable(ScriptTable)
            .InSchema("public")
            .Row(WreckScriptRow(
                "large",
                "wreck-unknown-origin-1",
                "wreck-unknown-origin-2"
            ))
            .Row(WreckScriptRow(
                "medium",
                "wreck-medium-1",
                "wreck-medium-2",
                "wreck-medium-3",
                "wreck-medium-4",
                "wreck-medium-5"
            ));

        Insert.IntoTable(ConstructDefinitionTable)
            .InSchema("public")
            .Row(WreckConstructDefRow("wreck-unknown-origin-1", "wrecks", "Wreck_5_UnknownOrigin.json",
                "Distress Signal"));
        Insert.IntoTable(ConstructDefinitionTable)
            .InSchema("public")
            .Row(WreckConstructDefRow("wreck-unknown-origin-2", "wrecks", "Wreck_5_UnknownOrigin.json",
                "Unknown Signal"));

        Insert.IntoTable(ConstructDefinitionTable)
            .InSchema("public")
            .Row(WreckConstructDefRow("wreck-medium-1", "wrecks", "Wreck_4_Shade.json", "Distress Signal"));

        Insert.IntoTable(ConstructDefinitionTable)
            .InSchema("public")
            .Row(WreckConstructDefRow("wreck-medium-2", "wrecks", "Wreck_4_Shade.json", "Battle Debris"));

        Insert.IntoTable(ConstructDefinitionTable)
            .InSchema("public")
            .Row(WreckConstructDefRow("wreck-medium-3", "wrecks", "Wreck_4_Phalanx.json", "Battle Debris"));

        Insert.IntoTable(ConstructDefinitionTable)
            .InSchema("public")
            .Row(WreckConstructDefRow("wreck-medium-4", "wrecks", "Wreck_4_Argos.json", "Unknown Signal"));

        Insert.IntoTable(ConstructDefinitionTable)
            .InSchema("public")
            .Row(WreckConstructDefRow("wreck-medium-5", "wrecks", "Wreck_4_Argos.json", "Faint Emissions"));
    }

    private object WreckScriptRow(string size, params string[] constructDefNames)
    {
        var constructDefNamesList = constructDefNames.ToList();

        return new
        {
            id = Guid.NewGuid(),
            name = $"spawn-poi-{size}-wreck",
            content = JsonConvert.SerializeObject(
                new ScriptActionItem
                {
                    Name = $"spawn-poi-{size}-wreck",
                    Type = "random",
                    Actions = constructDefNamesList.Select(constructDef =>
                        new ScriptActionItem
                        {
                            Area = new ScriptActionAreaItem(),
                            Type = "spawn",
                            Prefab = constructDef,
                            MinQuantity = 1,
                            MaxQuantity = 1
                        }
                    ).ToList()
                }
            )
        };
    }

    public object WreckConstructDefRow(string name, string folder, string file, string shipName)
    {
        return new
        {
            id = Guid.NewGuid(),
            name,
            content = JsonConvert.SerializeObject(
                new PrefabItem
                {
                    Name = name,
                    Folder = folder,
                    Path = file,
                    OwnerId = 0,
                    ServerProperties = new PrefabItem.ServerPropertiesItem
                    {
                        IsDynamicWreck = true,
                        Header = new PrefabItem.ServerPropertiesItem.HeaderProp
                        {
                            PrettyName = shipName
                        }
                    }
                }
            )
        };
    }

    public override void Down()
    {
    }
}