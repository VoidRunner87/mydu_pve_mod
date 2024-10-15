using FluentMigrator;

namespace Orleans.Migrations;

[Migration(1)]
public class AddIndexToOrleansToFixDuplicateSilos : Migration
{
    public override void Up()
    {
        // "Fixes" an issue in orleans with spawning constructs by enforcing unique constraint on the orleans storage table.
        Execute.Sql(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS idx_grainidn1_graintypestring_partial_unique
            ON public.storage (grainidn0, grainidn1, graintypestring)
            WHERE graintypestring = 'NQ.Grains.Gameplay.ConstructInfoGrain';
            """
        );
    }

    public override void Down()
    {
        
    }
}