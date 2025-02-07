using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(31)]
public class AddWebhooksTable : Migration
{
    private const string WebhookTable = "mod_webhook";
    
    public override void Up()
    {
        Create.Table(WebhookTable)
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewGuid).PrimaryKey()
            .WithColumn("url").AsString().NotNullable()
            .WithColumn("active").AsBoolean().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTimeUtc().WithDefault(SystemMethods.CurrentUTCDateTime);
    }

    public override void Down()
    {
        Delete.Table(WebhookTable);
    }
}