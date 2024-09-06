using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(20)]
public class AddSectorMetricsAndFieldsForPublishDateAndFixDates : Migration
{
    private const string ModEvent = "mod_event";
    private const string ModEventTrigger = "mod_event_trigger";
    private const string ModEventTriggerTracker = "mod_event_trigger_tracker";
    
    private const string ModSectorInstanceTable = "mod_sector_instance";
    private const string ScriptTable = "mod_script";
    private const string ModSpawnerTaskQueueTable = "mod_task_queue";
    private const string NpcConstructHandleTable = "mod_npc_construct_handle";

    public override void Up()
    {
        Create.Table(ModEvent)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("event_name").AsString().NotNullable()
            .WithColumn("event_data").AsCustom("jsonb").Nullable().Indexed()
            .WithColumn("value").AsDouble().WithDefaultValue(1).NotNullable()
            .WithColumn("player_id").AsInt64().ForeignKey("player", "id").Nullable()
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime).NotNullable();

        Create.Table(ModEventTrigger)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("event_name").AsString().NotNullable().Indexed()
            .WithColumn("min_trigger_value").AsDouble().NotNullable().WithDefaultValue(1)
            .WithColumn("player_id").AsInt64().ForeignKey("player", "id").Nullable()
            .WithColumn("on_trigger_script").AsString()
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Table(ModEventTriggerTracker)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("event_trigger_id").AsGuid().ForeignKey(ModEventTrigger, "id")
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime);
        
        Alter.Table(ModSectorInstanceTable)
            .InSchema("public")
            .AddColumn("publish_at").AsDateTime().Nullable()
            .AlterColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime);

        Alter.Table(ScriptTable)
            .InSchema("public")
            .AlterColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime)
            .AlterColumn("updated_at").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime);
        
        Alter.Table(NpcConstructHandleTable)
            .InSchema("public")
            .AlterColumn("last_controlled_at").AsDateTime().WithDefault(SystemMethods.CurrentUTCDateTime);

        Alter.Table(ModSpawnerTaskQueueTable)
            .AlterColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime).NotNullable();
    }

    public override void Down()
    {
        Delete.Table(ModEvent);
        Delete.Table(ModEventTrigger);

        Delete.Column("publish_at").FromTable(ModSectorInstanceTable);
    }
}

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