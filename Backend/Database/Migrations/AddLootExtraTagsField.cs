using System;
using FluentMigrator;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(47)]
public class AddLootExtraTagsField : Migration
{
    private const string LootDefinitionTable = "mod_loot_def";
    
    public override void Up()
    {
        Alter.Table(LootDefinitionTable)
            .AddColumn("extra_tags").AsCustom("jsonb").NotNullable()
            .WithDefaultValue(JsonConvert.SerializeObject(Array.Empty<object>()));
    }

    public override void Down()
    {
        Delete.Column("extra_tags")
            .FromTable(LootDefinitionTable);
    }
}