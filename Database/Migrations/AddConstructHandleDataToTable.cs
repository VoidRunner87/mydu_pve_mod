using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(14)]
public class AddConstructHandleDataToTable : Migration
{
    private const string NpcConstructHandleTable = "mod_npc_construct_handle";
    
    public override void Up()
    {
        Alter.Table(NpcConstructHandleTable)
            .InSchema("public")
            .AddColumn("original_owner_player_id").AsInt64().WithDefaultValue(0)
            .AddColumn("original_organization_id").AsInt64().WithDefaultValue(0)
            .AddColumn("on_cleanup_script").AsString().Nullable();
    }

    public override void Down()
    {
        Delete.Column("original_owner_player_id")
            .Column("original_organization_id")
            .Column("on_cleanup_script")
            .FromTable(NpcConstructHandleTable);
    }
}