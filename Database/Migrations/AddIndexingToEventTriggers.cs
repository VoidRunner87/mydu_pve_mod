using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(22)]
public class AddIndexingToEventTriggers : Migration
{
    private const string ModEvent = "mod_event";
    private const string ModEventTriggerTracker = "mod_event_trigger_tracker";
    
    public override void Up()
    {
        Alter.Table(ModEvent)
            .AlterColumn("event_name").AsString().NotNullable().Indexed();

        Create.Index("IX_mod_event_trigger_tracker_event_name_player_id")
            .OnTable(ModEventTriggerTracker)
            .InSchema("public")
            .OnColumn("event_trigger_id")
            .Ascending()
            .OnColumn("player_id")
            .Unique();
    }

    public override void Down()
    {
        Delete.Index("IX_mod_event_trigger_tracker_event_name_player_id");
    }
}