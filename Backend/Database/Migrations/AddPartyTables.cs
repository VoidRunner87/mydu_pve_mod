using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(43)]
public class AddPartyTables : Migration
{
    private const string PartyTable = "mod_player_party";
    
    public override void Up()
    {
        Create.Table(PartyTable)
            .WithColumn("id").AsGuid().PrimaryKey().WithDefault(SystemMethods.NewGuid)
            .WithColumn("group_id").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable().Indexed()
            .WithColumn("player_id").AsInt64().NotNullable().Unique()
            .WithColumn("is_leader").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_pending_accept_request").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_pending_accept_invite").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("json_properties").AsCustom("jsonb").NotNullable().WithDefaultValue(JsonConvert.SerializeObject(new {}))
            .WithColumn("created_at").AsDateTimeUtc().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
    }

    public override void Down()
    {
        Delete.Table(PartyTable);
    }
}