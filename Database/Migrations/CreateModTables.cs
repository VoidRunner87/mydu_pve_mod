using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(5)]
public class CreateModTables : Migration
{
    private const string ModSpawnerTaskQueueTable = "mod_task_queue";
    private const string ModFeaturesTable = "mod_features";
    private const string ModSectorInstanceTable = "mod_sector_instance";
    private const string ModAiConstructTrackerTable = "mod_ai_construct_tracker";
    private const string ModAiSectorIndexName = "IX_mod_ai_construct_tracker_sector";
    private const string ModSectorInstanceIndexName = "IX_mod_sector_instance_sector";
    
    private const string FieldSectorX = "sector_x";
    private const string FieldSectorY = "sector_y";
    private const string FieldSectorZ = "sector_z";

    public override void Up()
    {
        Create.Table(ModAiConstructTrackerTable)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn(FieldSectorX).AsInt64().NotNullable()
            .WithColumn(FieldSectorY).AsInt64().NotNullable()
            .WithColumn(FieldSectorZ).AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("last_control_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime);
        
        Create.Index(ModAiSectorIndexName).OnTable(ModAiConstructTrackerTable)
            .InSchema("public")
            .OnColumn(FieldSectorX).Ascending()
            .OnColumn(FieldSectorY).Ascending()
            .OnColumn(FieldSectorZ).Ascending();
        
        Create.Table(ModSectorInstanceTable)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn(FieldSectorX).AsInt64().NotNullable()
            .WithColumn(FieldSectorY).AsInt64().NotNullable()
            .WithColumn(FieldSectorZ).AsInt64().NotNullable()
            .WithColumn("on_load_script").AsString().NotNullable()
            .WithColumn("on_sector_enter_script").AsString().NotNullable()
            .WithColumn("loaded_at").AsDateTime().Nullable()
            .WithColumn("started_at").AsDateTime().Nullable()
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("expires_at").AsDateTime();
        
        Create.Index(ModSectorInstanceIndexName).OnTable(ModSectorInstanceTable)
            .InSchema("public")
            .OnColumn(FieldSectorX).Ascending()
            .OnColumn(FieldSectorY).Ascending()
            .OnColumn(FieldSectorZ).Ascending();
        
        Create.Table(ModFeaturesTable)
            .InSchema("public")
            .WithColumn("name").AsString(100).NotNullable().PrimaryKey()
            .WithColumn("type").AsString(20).NotNullable().WithDefaultValue("string")
            .WithColumn("value").AsString();

        Create.Table(ModSpawnerTaskQueueTable)
            .InSchema("public")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("command").AsString().NotNullable()
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime).NotNullable()
            .WithColumn("delivery_at").AsDateTime().NotNullable()
            .WithColumn("data").AsCustom("jsonb").NotNullable()
            .WithColumn("status").AsString().NotNullable()
            .WithColumn("updated_at").AsDateTime().Nullable();

        Insert.IntoTable(ModFeaturesTable)
            .InSchema("public")
            .Row(new
            {
                name = $"{nameof(AlienCoreRotationLoop)}Enabled",
                type = "bool",
                value = "false"
            });
    }

    public override void Down()
    {
        Delete.Table(ModSpawnerTaskQueueTable);
        Delete.Table(ModFeaturesTable);
        Delete.Table(ModSectorInstanceTable);
        Delete.Table(ModAiConstructTrackerTable);
    }
}