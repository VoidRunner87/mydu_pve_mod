using FluentMigrator;
using Mod.DynamicEncounters.Features.Sector.Data;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(23)]
public class AddSectorEncounterParameters : Migration
{
    private const string ModSectorInstanceTable = "mod_sector_encounter";
    
    public override void Up()
    {
        Alter.Table(ModSectorInstanceTable)
            .AddColumn("json_properties").AsCustom("jsonb")
            .NotNullable()
            .WithDefaultValue(
                JsonConvert.SerializeObject(
                    new EncounterProperties()
                )
            );
    }

    public override void Down()
    {
        Delete.Column("type")
            .Column("json_properties")
            .FromTable(ModSectorInstanceTable);
    }
}