using FluentMigrator;
using Mod.DynamicEncounters.Features.Sector.Services;

namespace Mod.DynamicEncounters.Database.Migrations;

/// <summary>
/// Adds Sector fields to make construct queries on sectors more performant
/// </summary>
[Migration(1)]
public class AddSectorFields : Migration
{
    private const string ConstructTable = "construct";
    private const string SectorIndexName = "IX_construct_sector";
    private const string FieldSectorX = "sector_x";
    private const string FieldSectorY = "sector_y";
    private const string FieldSectorZ = "sector_z";

    public override void Up()
    {
        Alter.Table(ConstructTable)
            .AddColumn(FieldSectorX).AsInt64().Nullable()
            .AddColumn(FieldSectorY).AsInt64().Nullable()
            .AddColumn(FieldSectorZ).AsInt64().Nullable();

        Create.Index(SectorIndexName).OnTable(ConstructTable)
            .InSchema("public")
            .OnColumn(FieldSectorX).Ascending()
            .OnColumn(FieldSectorY).Ascending()
            .OnColumn(FieldSectorZ).Ascending();

        Execute.Sql($"""
                    CREATE OR REPLACE FUNCTION fn_update_sector_columns()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        NEW.sector_x := round(NEW.position_x / {SectorPoolManager.SectorGridSnap}) * {SectorPoolManager.SectorGridSnap};
                        NEW.sector_y := round(NEW.position_y / {SectorPoolManager.SectorGridSnap}) * {SectorPoolManager.SectorGridSnap};
                        NEW.sector_z := round(NEW.position_z / {SectorPoolManager.SectorGridSnap}) * {SectorPoolManager.SectorGridSnap};
                    
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DROP TRIGGER IF EXISTS trigger_update_sector_columns ON public.{ConstructTable};
                    
                    CREATE TRIGGER trigger_update_sector_columns
                    BEFORE UPDATE ON public.construct
                    FOR EACH ROW
                    EXECUTE FUNCTION fn_update_sector_columns();

                    """);
    }

    public override void Down()
    {
        Delete.Column(FieldSectorX)
            .Column(FieldSectorY)
            .Column(FieldSectorZ)
            .FromTable(ConstructTable);

        Execute.Sql($"DROP TRIGGER IF EXISTS trigger_update_sector_columns ON public.{ConstructTable}");
    }
}