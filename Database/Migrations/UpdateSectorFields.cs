using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(19)]
public class UpdateSectorFields : Migration
{
    private const string ConstructTable = "construct";
    private const string ModSectorInstanceTable = "mod_sector_instance";
    private const string NpcConstructHandleTable = "mod_npc_construct_handle";
    
    private const string FieldSectorX = "sector_x";
    private const string FieldSectorY = "sector_y";
    private const string FieldSectorZ = "sector_z";

    public override void Up()
    {
        Alter.Table(ConstructTable)
            .AlterColumn(FieldSectorX).AsDouble().Nullable()
            .AlterColumn(FieldSectorY).AsDouble().Nullable()
            .AlterColumn(FieldSectorZ).AsDouble().Nullable();
        
        Alter.Table(ModSectorInstanceTable)
            .AlterColumn(FieldSectorX).AsDouble().Nullable()
            .AlterColumn(FieldSectorY).AsDouble().Nullable()
            .AlterColumn(FieldSectorZ).AsDouble().Nullable();
        
        Alter.Table(NpcConstructHandleTable)
            .AlterColumn(FieldSectorX).AsDouble().Nullable()
            .AlterColumn(FieldSectorY).AsDouble().Nullable()
            .AlterColumn(FieldSectorZ).AsDouble().Nullable();

        Delete.Table("mod_ai_construct_tracker");
    }

    public override void Down()
    {
        // breaking change
    }
}