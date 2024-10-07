using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(38)]
public class AddFactionTerritorySectorSize : Migration
{
    private const string FactionTable = "mod_faction";
    private const string TerritoryTable = "mod_territory";
    private const string FactionTerritoryTable = "mod_faction_territory";
    
    public override void Up()
    {
        Alter.Table(FactionTerritoryTable)
            .InSchema("public")
            .AddColumn("sector_count").AsInt32().NotNullable().WithDefaultValue(5);
    }

    public override void Down()
    {
        Delete.Column("sector_count")
            .FromTable(FactionTerritoryTable);
    }
}