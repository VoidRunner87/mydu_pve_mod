using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(34)]
public class AddTerritoryIdToSectorInstance : Migration
{
    private const string SectorInstanceTable = "mod_sector_instance";
    private const string TerritoryTable = "mod_territory";
    
    public override void Up()
    {
        Alter.Table(SectorInstanceTable)
            .AddColumn("territory_id").AsGuid().Nullable()
            .ForeignKey(TerritoryTable, "id");
    }

    public override void Down()
    {
        Delete.Column("territory_id")
            .FromTable(SectorInstanceTable);
    }
}