using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(36)]
public class AddTaskQueueDeletedAtField : Migration
{
    private const string TaskQueueTable = "mod_task_queue";

    public override void Up()
    {
        Alter.Table(TaskQueueTable)
            .AddColumn("deleted_at")
            .AsDateTimeUtc()
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("deleted_at")
            .FromTable(TaskQueueTable);
    }
}