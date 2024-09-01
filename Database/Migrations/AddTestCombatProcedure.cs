using FluentMigrator;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(16)]
public class AddTestCombatProcedure : Migration
{
    public override void Up()
    {
        Execute.Sql(
            """
            CREATE OR REPLACE FUNCTION sp_spawn_test_combat(
                pos_x double precision,
                pos_y double precision,
                pos_z double precision
            )
            RETURNS void AS $$
            BEGIN
            
            	INSERT INTO public.mod_task_queue (id, command, created_at, delivery_at, data, status, updated_at)
            	VALUES (
            		uuid_generate_v4(),
            		'script',
            		NOW(),
            		NOW(),
            		FORMAT('{"Type": "test-combat", "Position": { "x": %s, "y": %s, "z": %s }}', pos_x, pos_y, pos_z)::jsonb,
            		'scheduled',
            		NOW()
            	);
            	
            END;
            $$ LANGUAGE plpgsql;
            """
        );
    }

    public override void Down()
    {
        
    }
}