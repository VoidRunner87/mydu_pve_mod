using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(8)]
public class AddScriptsTable : Migration
{
    private const string ScriptTable = "mod_script";
    private const string ConstructDefinitionTable = "mod_construct_def";
    
    public override void Up()
    {
        Create.Table(ScriptTable)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString().Unique().NotNullable()
            .WithColumn("content").AsCustom("jsonb").NotNullable()
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime);

        // Delete.Index("IX_mod_script_name").OnTable(ScriptTable);
        // Create.Index("IX_mod_script_name").OnTable(ScriptTable)
        //     .InSchema("public")
        //     .OnColumn("name")
        //     .Ascending();
        
        Create.Table(ConstructDefinitionTable)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString().Unique().NotNullable()
            .WithColumn("content").AsCustom("jsonb").NotNullable()
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime);

        // Delete.Index("IX_mod_construct_def_name").OnTable(ScriptTable);
        // Create.Index("IX_mod_construct_def_name").OnTable(ScriptTable)
        //     .InSchema("public")
        //     .OnColumn("name")
        //     .Ascending();
    }

    public override void Down()
    {
        Delete.Table(ScriptTable);
        Delete.Table(ConstructDefinitionTable);
    }
}