using System;
using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(49)]
public class AddForceExpirationField : Migration
{
    private const string TerritoryTable = "mod_territory";
    
    public override void Up()
    {
        Alter.Table(TerritoryTable)
            .AddColumn("force_spawn_expiration_span").AsTime()
            .NotNullable().WithDefaultValue(TimeSpan.FromHours(6));
    }

    public override void Down()
    {
        Delete.Column("force_spawn_expiration_span")
            .FromTable(TerritoryTable);
    }
}