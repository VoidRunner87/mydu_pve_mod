using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(7)]
public class AddMoreFeatureEntries : Migration
{
    private const string ModFeaturesTable = "mod_features";
    
    public override void Up()
    {
        Insert.IntoTable(ModFeaturesTable)
            .InSchema("public")
            .Row(new
            {
                name = "SectorsToGenerate",
                type = "int",
                value = "10"
            });
        
        Insert.IntoTable(ModFeaturesTable)
            .InSchema("public")
            .Row(new
            {
                name = "ConstructHandleExpirationMinutes",
                type = "int",
                value = "360"
            });
        
        Insert.IntoTable(ModFeaturesTable)
            .InSchema("public")
            .Row(new
            {
                name = "ProcessQueueMessageCount",
                type = "int",
                value = "10"
            });
        
        Insert.IntoTable(ModFeaturesTable)
            .InSchema("public")
            .Row(new
            {
                name = $"{nameof(TaskQueueLoop)}Enabled",
                type = "bool",
                value = "true"
            });
        
        Insert.IntoTable(ModFeaturesTable)
            .InSchema("public")
            .Row(new
            {
                // left for compatibility with past migrations
                name = "HealthCheckLoopEnabled",
                type = "bool",
                value = "false"
            });
        
        Insert.IntoTable(ModFeaturesTable)
            .InSchema("public")
            .Row(new
            {
                name = $"{nameof(SectorLoop)}Enabled",
                type = "bool",
                value = "false"
            });
        
        Insert.IntoTable(ModFeaturesTable)
            .InSchema("public")
            .Row(new
            {
                name = $"{nameof(ConstructMovementBehaviorLoop)}Enabled",
                type = "bool",
                value = "false"
            });
    }

    public override void Down()
    {
        Delete.FromTable(ModFeaturesTable)
            .InSchema("public")
            .Row(new
            {
                name = "SectorsToGenerate"
            });
    }
}