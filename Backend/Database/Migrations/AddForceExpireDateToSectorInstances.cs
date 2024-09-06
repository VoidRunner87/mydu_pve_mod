using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(18)]
public class AddForceExpireDateToSectorInstances : Migration
{
    private const string ModSectorInstanceTable = "mod_sector_instance";
    
    public override void Up()
    {
        Alter.Table(ModSectorInstanceTable)
            .InSchema("public")
            .AddColumn("force_expire_at").AsDateTime().Nullable();
    }

    public override void Down()
    {
        Delete.Column("force_expire_at").FromTable(ModSectorInstanceTable);
    }
}