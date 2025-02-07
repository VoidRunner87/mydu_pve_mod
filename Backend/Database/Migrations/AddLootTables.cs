using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(27)]
public class AddLootTables : Migration
{
    private const string LootTable = "mod_loot_def";
    
    public override void Up()
    {
        Create.Table(LootTable)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey().WithDefault(SystemMethods.NewGuid)
            .WithColumn("name").AsString().Unique()
            .WithColumn("tags").AsCustom("jsonb").NotNullable()
            .WithColumn("items").AsCustom("jsonb").NotNullable()
            .WithColumn("created_at").AsDateTimeUtc().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("updated_at").AsDateTimeUtc().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("active").AsBoolean().WithDefaultValue(true);
    }

    public override void Down()
    {
        Delete.Table(LootTable);
    }
}