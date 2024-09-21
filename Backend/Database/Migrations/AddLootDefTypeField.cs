using System;
using FluentMigrator;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(32)]
public class AddLootDefTypeField : Migration
{
    private const string LootTable = "mod_loot_def";
    
    public override void Up()
    {
        Alter.Table(LootTable)
            .AddColumn("elements").AsCustom("jsonb").WithDefaultValue(
                JsonConvert.SerializeObject(Array.Empty<object>())
            );
    }

    public override void Down()
    {
        Delete.Column("elements").FromTable(LootTable);
    }
}