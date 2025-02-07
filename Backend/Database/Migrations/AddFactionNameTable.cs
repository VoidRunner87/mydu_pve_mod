using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(45)]
public class AddFactionNameTable : Migration
{
    public const string FactionNameTable = "mod_faction_name";
    
    public override void Up()
    {
        Create.Table(FactionNameTable)
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("group").AsString().NotNullable()
            .WithColumn("faction_id").AsInt64().NotNullable()
            ;
        
        Execute.Sql("CREATE INDEX idx_mod_faction_name ON mod_faction_name (name, faction_id);");
    }

    public override void Down()
    {
        Delete.Table(FactionNameTable);
        Delete.Index("idx_mod_faction_name");
    }
}