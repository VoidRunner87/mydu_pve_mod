using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(46)]
public class AddSectorInstanceNameField : Migration
{
    private const string SectorInstanceTable = "mod_sector_instance";
    
    public override void Up()
    {
        Alter.Table(SectorInstanceTable)
            .AddColumn("name").AsString().WithDefaultValue("Unknown Signal").NotNullable();
    }

    public override void Down()
    {
        Delete.Column("name").FromTable(SectorInstanceTable);
    }
}