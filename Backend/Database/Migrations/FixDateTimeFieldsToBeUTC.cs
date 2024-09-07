using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(24)]
public class FixDateTimeFieldsToBeUtc : Migration
{
    private const string SectorInstanceTable = "mod_sector_instance";
    private const string EventTable = "mod_event";
    private const string EventTriggerTable = "mod_event_trigger";
    private const string EventTriggerTrackerTable = "mod_event_trigger_tracker";
    private const string NpcConstructHandleTable = "mod_npc_construct_handle";
    private const string ScriptTable = "mod_script";
    private const string TakQueueTable = "mod_task_queue";
    
    public override void Up()
    {
        Alter
            .Column("created_at")
            .OnTable(TakQueueTable)
            .AsDateTimeUtc();
        
        Alter
            .Column("delivery_at")
            .OnTable(TakQueueTable)
            .AsDateTimeUtc();
        
        Alter
            .Column("updated_at")
            .OnTable(TakQueueTable)
            .AsDateTimeUtc();
        
        Alter
            .Column("created_at")
            .OnTable(EventTable)
            .AsDateTimeUtc();

        Alter
            .Column("created_at")
            .OnTable(EventTriggerTable)
            .AsDateTimeUtc();

        Alter
            .Column("created_at")
            .OnTable(EventTriggerTrackerTable)
            .AsDateTimeUtc();

        Alter
            .Column("last_controlled_at")
            .OnTable(NpcConstructHandleTable)
            .AsDateTimeUtc();
        
        Alter
            .Column("created_at")
            .OnTable(ScriptTable)
            .AsDateTimeUtc();
        
        Alter
            .Column("updated_at")
            .OnTable(ScriptTable)
            .AsDateTimeUtc();
        
        Alter
            .Column("loaded_at")
            .OnTable(SectorInstanceTable)
            .AsDateTimeUtc();
        
        Alter
            .Column("started_at")
            .OnTable(SectorInstanceTable)
            .AsDateTimeUtc();
        
        Alter
            .Column("created_at")
            .OnTable(SectorInstanceTable)
            .AsDateTimeUtc();
        
        Alter
            .Column("expires_at")
            .OnTable(SectorInstanceTable)
            .AsDateTimeUtc();
        
        Alter
            .Column("force_expire_at")
            .OnTable(SectorInstanceTable)
            .AsDateTimeUtc();
    }

    public override void Down()
    {
        
    }
}