using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(50)]
public class AddConstructGenericStateTable : Migration
{
    public const string ConstructStateTable = "mod_construct_state";
    
    public override void Up()
    {
        Create.Table(ConstructStateTable)
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewGuid).PrimaryKey()
            .WithColumn("construct_id").AsInt64().Indexed()
            .WithColumn("type").AsString()
            .WithColumn("properties").AsCustom("jsonb").NotNullable().WithDefaultValue("{}")
            .WithColumn("created_at").AsDateTimeUtc().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("updated_at").AsDateTimeUtc().WithDefault(SystemMethods.CurrentUTCDateTime);
    }

    public override void Down()
    {
        Delete.Table(ConstructStateTable);
    }
}