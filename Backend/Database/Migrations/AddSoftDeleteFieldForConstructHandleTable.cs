using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(33)]
public class AddSoftDeleteFieldForConstructHandleTable : Migration
{
    private const string NpcConstructHandleTable = "mod_npc_construct_handle";
    
    public override void Up()
    {
        Alter.Table(NpcConstructHandleTable)
            .AddColumn("deleted_at").AsDateTimeUtc().Nullable()
            .AlterColumn("id").AsGuid().PrimaryKey().NotNullable().WithDefault(SystemMethods.NewGuid);
    }

    public override void Down()
    {
        Delete.Column("deleted_at")
            .FromTable(NpcConstructHandleTable);
    }
}