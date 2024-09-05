using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(21)]
public class ChangeEventFields : Migration
{
    private const string ModEvent = "mod_event";
    private const string ModEventTrigger = "mod_event_trigger";
    private const string ModEventTriggerTracker = "mod_event_trigger_tracker";
    
    public override void Up()
    {
        Delete.Column("player_id").FromTable(ModEventTrigger);

        Alter.Table(ModEvent)
            .AlterColumn("event_data").AsCustom("jsonb").NotNullable();
        
        Alter.Table(ModEventTriggerTracker)
            .InSchema("public")
            .AddColumn("player_id").AsInt64().NotNullable().ForeignKey("player", "id");
    }

    public override void Down()
    {
        
    }
}