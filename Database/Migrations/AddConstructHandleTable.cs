using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(10)]
public class AddConstructHandleTable : Migration
{
    private const string NpcConstructHandleTable = "mod_npc_construct_handle";
    private const string FieldSectorX = "sector_x";
    private const string FieldSectorY = "sector_y";
    private const string FieldSectorZ = "sector_z";
    
    public override void Up()
    {
        Create.Table(NpcConstructHandleTable)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("construct_id").AsInt64()
            .WithColumn("construct_def_id").AsGuid()
                .ForeignKey("mod_construct_def", "id")
            .WithColumn(FieldSectorX).AsInt64().NotNullable()
            .WithColumn(FieldSectorY).AsInt64().NotNullable()
            .WithColumn(FieldSectorZ).AsInt64().NotNullable()
            .WithColumn("last_controlled_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime);
            
        Create.Index("IX_mod_npc_construct_handle_sector").OnTable(NpcConstructHandleTable)
            .InSchema("public")
            .OnColumn(FieldSectorX).Ascending()
            .OnColumn(FieldSectorY).Ascending()
            .OnColumn(FieldSectorZ).Ascending();
    }

    public override void Down()
    {
        Delete.Index("IX_mod_npc_construct_handle_sector").OnTable(NpcConstructHandleTable);
        Delete.Table(NpcConstructHandleTable);
    }
}