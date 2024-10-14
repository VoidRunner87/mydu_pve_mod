using FluentMigrator;
using Mod.DynamicEncounters.Features.Sector.Services;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(39)]
public class AddPostGisSetup : Migration
{
    private const string ConstructTable = "construct";

    public override void Up()
    {
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS postgis;");

        Execute.Sql(
            $"""
             ALTER TABLE public.{ConstructTable}
             ADD COLUMN position GEOMETRY(POINTZ, 0);
             """
        );

        Execute.Sql(
            $"""
             CREATE INDEX idx_construct_position_gist
             ON public.{ConstructTable}
             USING GIST (position);
             """
        );
        
        // Updates every construct's position field
        Execute.Sql(
            """
            UPDATE public.construct
            SET position = ST_MakePoint(position_x, position_y, position_z)
            """
        );

        Execute.Sql($"""
                     CREATE OR REPLACE FUNCTION fn_update_sector_columns()
                     RETURNS TRIGGER AS $$
                     BEGIN
                         NEW.sector_x := round(NEW.position_x / {SectorPoolManager.SectorGridSnap}) * {SectorPoolManager.SectorGridSnap};
                         NEW.sector_y := round(NEW.position_y / {SectorPoolManager.SectorGridSnap}) * {SectorPoolManager.SectorGridSnap};
                         NEW.sector_z := round(NEW.position_z / {SectorPoolManager.SectorGridSnap}) * {SectorPoolManager.SectorGridSnap};
                     
                         NEW.position := ST_MakePoint(NEW.position_x, NEW.position_y, NEW.position_z);
                     
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
        Delete.Column("position")
            .FromTable("construct");
    }
}