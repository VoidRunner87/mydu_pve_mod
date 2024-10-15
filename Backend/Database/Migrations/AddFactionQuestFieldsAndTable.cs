using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(41)]
public class AddMoreQuestFields : Migration
{
    private const string PlayerQuestTable = "mod_player_quest";
    private const string PlayerQuestTaskTable = "mod_player_quest_task";
    private const string PlayerQuestRewardTable = "mod_player_quest_reward";
    
    public override void Up()
    {
        Alter.Table(PlayerQuestTaskTable)
            .AddColumn("base_construct_id").AsInt64().Nullable();

        Alter.Table(PlayerQuestTable)
            .AddColumn("on_success_script").AsCustom("jsonb").Nullable()
            .AddColumn("on_failure_script").AsCustom("jsonb").Nullable();

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

        Delete.Table(PlayerQuestRewardTable);
    }
}

[Migration(40)]
public class AddFactionQuestFieldsAndTable : Migration
{
    private const string FactionTable = "mod_faction";
    private const string PlayerQuestTable = "mod_player_quest";
    private const string TerritoryQuestContainerTable = "mod_territory_quest_container";
    private const string TerritoryTable = "mod_territory";
    private const string PlayerQuestTaskTable = "mod_player_quest_task";
    
    public override void Up()
    {
        Create.Table(PlayerQuestTable)
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewGuid).PrimaryKey()
            .WithColumn("player_id").AsInt64().NotNullable()
            .WithColumn("faction_id").AsInt64().NotNullable().ForeignKey(FactionTable, "id")
            .WithColumn("type").AsString().NotNullable()
            .WithColumn("seed").AsInt32().NotNullable()
            .WithColumn("json_properties").AsCustom("jsonb").NotNullable()
            .WithColumn("created_at").AsDateTimeUtc().NotNullable()
            .WithColumn("expires_at").AsDateTimeUtc().NotNullable();

        Create.Table(PlayerQuestTaskTable)
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewGuid).PrimaryKey()
            .WithColumn("quest_id").AsGuid().ForeignKey(PlayerQuestTable, "id")
            .WithColumn("text").AsString()
            .WithColumn("type").AsString().NotNullable()
            .WithColumn("status").AsString().NotNullable()
            .WithColumn("position_x").AsDouble().NotNullable()
            .WithColumn("position_y").AsDouble().NotNullable()
            .WithColumn("position_z").AsDouble().NotNullable()
            .WithColumn("json_properties").AsCustom("jsonb").NotNullable()
            .WithColumn("on_success_script").AsCustom("jsonb").Nullable()
            .WithColumn("on_failure_script").AsCustom("jsonb").Nullable()
            .WithColumn("completed_at").AsDateTimeUtc().Nullable();

        Create.Table(TerritoryQuestContainerTable)
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewGuid).PrimaryKey()
            .WithColumn("territory_id").AsGuid().ForeignKey(TerritoryTable, "id")
            .WithColumn("construct_id").AsInt64().NotNullable()
            .WithColumn("element_id").AsInt64().NotNullable();
    }

    public override void Down()
    {
        Delete.Column("seed")
            .FromTable(FactionTable);

        Delete.Table(TerritoryQuestContainerTable);
        Delete.Table(PlayerQuestTaskTable);
        Delete.Table(PlayerQuestTable);
    }
}