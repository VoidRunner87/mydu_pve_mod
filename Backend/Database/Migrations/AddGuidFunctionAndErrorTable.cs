using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(25)]
public class AddGuidFunctionAndErrorTable : Migration
{
    private const string ErrorTable = "mod_error";
    
    public override void Up()
    {
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");

        Create.Table(ErrorTable)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("type").AsString().NotNullable()
            .WithColumn("subtype").AsString().Nullable()
            .WithColumn("error").AsString().NotNullable()
            .WithColumn("created_at").AsDateTimeUtc().WithDefault(SystemMethods.CurrentUTCDateTime);
    }

    public override void Down()
    {
        
    }
}