using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(48)]
public class AddMissingDefaultGuids : Migration
{
    private const string SectorEncounterTable = "mod_sector_encounter";
    private const string ScriptTable = "mod_script";
    private const string ConstructDefinitionTable = "mod_construct_def";
    
    public override void Up()
    {
        Alter.Column("id")
            .OnTable(SectorEncounterTable)
            .AsGuid()
            .WithDefault(SystemMethods.NewGuid);
        
        Alter.Column("id")
            .OnTable(ScriptTable)
            .AsGuid()
            .WithDefault(SystemMethods.NewGuid);
        
        Alter.Column("id")
            .OnTable(ConstructDefinitionTable)
            .AsGuid()
            .WithDefault(SystemMethods.NewGuid);
    }

    public override void Down()
    {
        
    }
}