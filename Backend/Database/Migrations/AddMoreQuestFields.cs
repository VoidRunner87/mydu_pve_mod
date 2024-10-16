using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(41)]
public class AddMoreQuestFields : Migration
{
    private const string PlayerQuestTable = "mod_player_quest";
    private const string PlayerQuestTaskTable = "mod_player_quest_task";
    
    public override void Up()
    {
        Alter.Table(PlayerQuestTaskTable)
            .AddColumn("base_construct_id").AsInt64().Nullable();

        Alter.Table(PlayerQuestTable)
            .AddColumn("on_success_script").AsCustom("jsonb").Nullable()
            .AddColumn("on_failure_script").AsCustom("jsonb").Nullable()
            .AddColumn("deleted_at").AsDateTimeUtc().Nullable()
            .AddColumn("status").AsString().NotNullable();

        Delete.Column("on_success_script")
            .Column("on_failure_script")
            .FromTable(PlayerQuestTaskTable);

        Alter.Table(PlayerQuestTaskTable)
            .AddColumn("on_check_completed_script").AsCustom("jsonb").Nullable();
    }

    public override void Down()
    {
        Delete.Column("base_construct_id")
            .FromTable(PlayerQuestTaskTable);
    }
}