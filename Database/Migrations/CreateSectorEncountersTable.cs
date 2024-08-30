using System;
using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(6)]
public class CreateSectorEncountersTable : Migration
{
    private const string ModSectorEncounterTable = "mod_sector_encounter";
    
    public override void Up()
    {
        Create.Table(ModSectorEncounterTable)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(150)
            .WithColumn("on_load_script").AsString(100).Nullable()
            .WithColumn("on_sector_enter_script").AsString(100).Nullable()
            .WithColumn("active").AsBoolean().WithDefaultValue(true);

        Insert.IntoTable(ModSectorEncounterTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Easy Encounter",
                on_load_script = "spawn-poi-easy-encounter",
                on_sector_enter_script = "spawn-easy-encounter",
                active = true
            });
        
        Insert.IntoTable(ModSectorEncounterTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Medium Encounter",
                on_load_script = "spawn-poi-medium-encounter",
                on_sector_enter_script = "spawn-medium-encounter",
                active = true
            });
        
        Insert.IntoTable(ModSectorEncounterTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Hard Encounter",
                on_load_script = "spawn-poi-hard-encounter",
                on_sector_enter_script = "spawn-hard-encounter",
                active = true
            });
    }

    public override void Down()
    {
        Delete.Table(ModSectorEncounterTable);
    }
}