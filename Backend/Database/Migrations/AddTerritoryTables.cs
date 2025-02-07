using System;
using FluentMigrator;
using Mod.DynamicEncounters.Database.Helpers;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(30)]
public class AddTerritoryTables : Migration
{
    private const string FactionTable = "mod_faction";
    private const string TerritoryTable = "mod_territory";
    private const string FactionTerritoryTable = "mod_faction_territory";
    private const string SectorEncounterTable = "mod_sector_encounter";
    private const string SectorInstanceTable = "mod_sector_instance";
    private const string NpcConstructHandleTable = "mod_npc_construct_handle";
    
    public override void Up()
    {
        Create.Table(TerritoryTable)
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewGuid).PrimaryKey()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("spawn_position_x").AsDouble().NotNullable().WithDefaultValue(13771471)
            .WithColumn("spawn_position_y").AsDouble().NotNullable().WithDefaultValue(7435803)
            .WithColumn("spawn_position_z").AsDouble().NotNullable().WithDefaultValue(-128971)
            .WithColumn("spawn_min_radius").AsDouble().NotNullable().WithDefaultValue(130 * 200 * 1000)
            .WithColumn("spawn_max_radius").AsDouble().NotNullable().WithDefaultValue(200 * 200 * 1000)
            .WithColumn("spawn_expiration_span").AsTime().NotNullable().WithDefaultValue(TimeSpan.FromHours(3))
            .WithColumn("active").AsBoolean().NotNullable().WithDefaultValue(true);

        var defaultTerritoryGuid = Guid.NewGuid();
        Insert.IntoTable(TerritoryTable)
            .InSchema("public")
            .Row(new
            {
                id = defaultTerritoryGuid,
                name = "default"
            });
        
        Create.Table(FactionTerritoryTable)
            .WithColumn("id").AsGuid().WithDefault(SystemMethods.NewGuid).PrimaryKey()
            .WithColumn("faction_id").AsInt64().ForeignKey(FactionTable, "id").NotNullable()
            .WithColumn("territory_id").AsGuid().ForeignKey(TerritoryTable, "id").NotNullable()
            .WithColumn("permanent").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("updated_at").AsDateTimeUtc().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Alter.Table(SectorEncounterTable)
            .AddColumn("territory_id").AsGuid().ForeignKey(TerritoryTable, "id")
            .WithDefaultValue(defaultTerritoryGuid)
            .AddColumn("faction_id").AsInt64()
            .ForeignKey(FactionTable, "id")
            .WithDefaultValue(1);

        Alter.Table(SectorInstanceTable)
            .AddColumn("faction_id").AsInt64()
            .ForeignKey(FactionTable, "id")
            .NotNullable()
            .WithDefaultValue(1);

        Alter.Table(NpcConstructHandleTable)
            .AddColumn("faction_id").AsInt64()
            .NotNullable()
            .ForeignKey(FactionTable, "id")
            .WithDefaultValue(1);
    }

    public override void Down()
    {
        Delete.Column("territory_id")
            .Column("faction_id")
            .FromTable(SectorEncounterTable);

        Delete.Column("faction_id")
            .FromTable(SectorInstanceTable);
        
        Delete.Column("faction_id")
            .FromTable(NpcConstructHandleTable);
        
        Delete.Table(FactionTerritoryTable);
        Delete.Table(TerritoryTable);
    }
}