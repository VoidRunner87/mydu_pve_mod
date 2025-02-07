using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(26)]
public class AddFeatureToggleForCombatLockResetPVE : Migration
{
    private const string FeaturesTable = "mod_features";
    
    public override void Up()
    {
        Insert.IntoTable(FeaturesTable)
            .InSchema("public")
            .Row(new
            {
                name = "ResetNPCCombatLockOnDestruction",
                type = "bool",
                value = "false"
            });
    }

    public override void Down()
    {
        Delete.FromTable(FeaturesTable)
            .InSchema("public")
            .Row(new
            {
                name = "ResetNPCCombatLockOnDestruction"
            });
    }
}