using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(35)]
public class AddNpcConstructHandleCreatedDate : Migration
{
    private const string TerritoryTable = "mod_territory";
    private const string NpcConstructHandleTable = "mod_npc_construct_handle";
    
    public override void Up()
    {
        Alter.Table(NpcConstructHandleTable)
            .AddColumn("territory_id").AsGuid().ForeignKey(TerritoryTable, "id")
            .Nullable()
            .AddColumn("created_at").AsDateTimeUtc().WithDefault(SystemMethods.CurrentUTCDateTime)
            .NotNullable();
    }

    public override void Down()
    {
        Delete.Column("created_at")
            .FromTable(NpcConstructHandleTable);
    }
}