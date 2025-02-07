using System;
using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(13)]
public class UpdateSectorEncountersForWrecksFirst : Migration
{
    private const string SectorEncounterTable = "mod_sector_encounter";
    
    public override void Up()
    {
        Delete.FromTable(SectorEncounterTable)
            .InSchema("public")
            .AllRows();

        Insert.IntoTable(SectorEncounterTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Medium Wreck",
                on_load_script = "spawn-poi-medium-wreck",
                active = true
            });
        
        Insert.IntoTable(SectorEncounterTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Large Wreck",
                on_load_script = "spawn-poi-large-wreck",
                active = true
            });
    }

    public override void Down()
    {
        
    }
}