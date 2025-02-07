using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(17)]
public class AddConstructHandleProperties : Migration
{
    private const string NpcConstructHandleTable = "mod_npc_construct_handle";
    
    public override void Up()
    {
        Alter.Table(NpcConstructHandleTable)
            .InSchema("public")
            .AddColumn("json_properties").AsCustom("jsonb").Nullable();
    }

    public override void Down()
    {
        Delete.Column("json_properties").FromTable(NpcConstructHandleTable);
    }
}