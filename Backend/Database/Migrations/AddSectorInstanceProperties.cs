using FluentMigrator;
using Mod.DynamicEncounters.Features.Sector.Data;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(44)]
public class AddSectorInstanceProperties : Migration
{
    private const string SectorInstanceTable = "mod_sector_instance";
    
    public override void Up()
    {
        Alter.Table(SectorInstanceTable)
            .AddColumn("json_properties").AsCustom("jsonb")
            .NotNullable()
            .WithDefaultValue(JsonConvert.SerializeObject(new SectorInstanceProperties()));
    }

    public override void Down()
    {
        Delete.Column("json_properties")
            .FromTable(SectorInstanceTable);
    }
}