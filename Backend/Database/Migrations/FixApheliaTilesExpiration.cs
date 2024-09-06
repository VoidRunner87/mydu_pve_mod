using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

/// <summary>
/// Some aphelia tiles will expire if we don't fix them.
/// Also added a lot of quanta to their balances, so they won't expire
/// </summary>
[Migration(4)]
public class FixApheliaTilesExpiration : Migration
{
    public override void Up()
    {
        Execute.Sql(
            """
            UPDATE public.territory
                SET 
                    expires_at = '3000-01-01',
                    balance = 999999999999
            WHERE owner_entity_id = 2;
            """
        );
    }

    public override void Down()
    {
    }
}