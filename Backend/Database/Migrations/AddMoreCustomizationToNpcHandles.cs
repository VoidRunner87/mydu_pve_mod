using FluentMigrator;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Sector.Data;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(29)]
public class AddMoreCustomizationToNpcHandles : Migration
{
    private const string FactionTable = "mod_faction";
    private const string SectorInstanceTable = "mod_sector_instance";

    public override void Up()
    {
        Alter.Table(SectorInstanceTable)
            .AddColumn("json_properties").AsCustom("jsonb").NotNullable()
            .WithDefaultValue(
                JsonConvert.SerializeObject(
                    new SectorInstance.SectorInstanceProperties
                    {
                        Tags = ["pooled"]
                    }
                )
            );

        Create.Table(FactionTable)
            .WithColumn("id").AsString().NotNullable().PrimaryKey()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("organization_id").AsInt64().Nullable()
            .ForeignKey("organization", "id")
            .WithColumn("json_properties").AsCustom("jsonb")
            .WithDefaultValue(
                JsonConvert.SerializeObject(new FactionItem.FactionProperties())
            );

        Insert.IntoTable(FactionTable)
            .InSchema("public")
            .Row(new
            {
                id = "pirates",
                name = "Pirates",
                json_properties = JsonConvert.SerializeObject(
                    new FactionItem.FactionProperties
                    {
                        SectorPoolCount = 10
                    }
                )
            });
        
        Insert.IntoTable(FactionTable)
            .InSchema("public")
            .Row(new
            {
                id = "uef",
                name = "United Earth Force",
                json_properties = JsonConvert.SerializeObject(
                    new FactionItem.FactionProperties
                    {
                        SectorPoolCount = 4
                    }
                )
            });
    }

    public override void Down()
    {
    }
}