using FluentMigrator;
using Mod.DynamicEncounters.Features.Faction.Data;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(29)]
public class AddMoreCustomizationToNpcHandles : Migration
{
    private const string FactionTable = "mod_faction";
    private const string SectorInstanceTable = "mod_sector_instance";

    public override void Up()
    {
        Create.Table(FactionTable)
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("tag").AsString().Unique()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("player_id").AsInt64().NotNullable()
            .WithDefaultValue(4)
            .ForeignKey("player", "id")
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
                tag = "pirates",
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
                tag = "uef",
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
        Delete.Table(FactionTable);
    }
}